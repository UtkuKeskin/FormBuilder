using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FormBuilder.Core.Entities;
using FormBuilder.Core.Interfaces;
using FormBuilder.Web.ViewModels.Form;
using System.Security.Claims;

namespace FormBuilder.Web.Controllers
{
    public class FormController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITemplateService _templateService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<FormController> _logger;

        public FormController(
            IUnitOfWork unitOfWork,
            ITemplateService templateService,
            UserManager<User> userManager,
            ILogger<FormController> logger)
        {
            _unitOfWork = unitOfWork;
            _templateService = templateService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Form/Fill/{templateId}
        [AllowAnonymous]
        public async Task<IActionResult> Fill(int templateId)
        {
            var template = await _templateService.GetTemplateByIdAsync(templateId);
            if (template == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Access control
            if (!template.IsPublic)
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
                    
                if (!await _templateService.CanUserAccessTemplateAsync(templateId, userId))
                    return Forbid();
            }

            var model = new FillFormViewModel
            {
                TemplateId = template.Id,
                TemplateTitle = template.Title,
                TemplateDescription = template.Description,
                IsAnonymous = !User.Identity.IsAuthenticated,
                Questions = GetActiveQuestions(template)
            };

            return View(model);
        }

        // POST: /Form/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitFormViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Create new form submission
            var form = new Form
            {
                TemplateId = model.TemplateId,
                UserId = userId,
                FilledAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                
                // Map answers
                CustomString1Answer = model.CustomString1Answer ?? "",
                CustomString2Answer = model.CustomString2Answer ?? "",
                CustomString3Answer = model.CustomString3Answer ?? "",
                CustomString4Answer = model.CustomString4Answer ?? "",
                
                CustomText1Answer = model.CustomText1Answer ?? "",
                CustomText2Answer = model.CustomText2Answer ?? "",
                CustomText3Answer = model.CustomText3Answer ?? "",
                CustomText4Answer = model.CustomText4Answer ?? "",
                
                CustomInt1Answer = model.CustomInt1Answer,
                CustomInt2Answer = model.CustomInt2Answer,
                CustomInt3Answer = model.CustomInt3Answer,
                CustomInt4Answer = model.CustomInt4Answer,
                
                CustomCheckbox1Answer = model.CustomCheckbox1Answer,
                CustomCheckbox2Answer = model.CustomCheckbox2Answer,
                CustomCheckbox3Answer = model.CustomCheckbox3Answer,
                CustomCheckbox4Answer = model.CustomCheckbox4Answer
            };

            await _unitOfWork.Forms.AddAsync(form);
            await _unitOfWork.SaveAsync();

            TempData["Success"] = "Form submitted successfully!";
            return RedirectToAction("Success", new { id = form.Id });
        }

        // GET: /Form/Success/{id}
        [Authorize]
        public async Task<IActionResult> Success(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var form = await _unitOfWork.Forms.GetByIdAsync(id);
            
            if (form == null || form.UserId != userId)
                return NotFound();

            return View(form);
        }

        // GET: /Form/MyForms
        [Authorize]
        public async Task<IActionResult> MyForms(int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var forms = await _unitOfWork.Forms.GetUserFormsAsync(userId);
            
            var model = new MyFormsViewModel
            {
                Forms = forms.Select(f => new FormListItemViewModel
                {
                    Id = f.Id,
                    TemplateTitle = f.Template.Title,
                    FilledAt = f.FilledAt,
                    TemplateId = f.TemplateId,
                    DisplayFields = GetDisplayFields(f)
                }).ToList()
            };

            return View(model);
        }

        // GET: /Form/View/{id}
        [Authorize]
        public async Task<IActionResult> View(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            var form = await _unitOfWork.Forms.GetByIdAsync(id);
            if (form == null) return NotFound();

            // Access control
            var template = await _templateService.GetTemplateByIdAsync(form.TemplateId);
            var canView = form.UserId == userId || 
                         template.UserId == userId || 
                         isAdmin;
                         
            if (!canView) return Forbid();

            var model = new ViewFormViewModel
            {
                Form = form,
                Template = template,
                Answers = GetFormAnswers(form, template)
            };

            return View(model);
        }

        // POST: /Form/Delete/{id}
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            var form = await _unitOfWork.Forms.GetByIdAsync(id);
            if (form == null) return NotFound();
            
            if (form.UserId != userId && !isAdmin)
                return Forbid();

            _unitOfWork.Forms.Delete(form);
            await _unitOfWork.SaveAsync();

            TempData["Success"] = "Form deleted successfully!";
            return RedirectToAction(nameof(MyForms));
        }

        // Helper methods
        private List<QuestionViewModel> GetActiveQuestions(Template template)
        {
            var questions = new List<QuestionViewModel>();
            int order = 1;

            // String questions
            for (int i = 1; i <= 4; i++)
            {
                if ((bool)template.GetType().GetProperty($"CustomString{i}State")?.GetValue(template))
                {
                    questions.Add(new QuestionViewModel
                    {
                        Type = "string",
                        FieldName = $"CustomString{i}Answer",
                        Question = template.GetType().GetProperty($"CustomString{i}Question")?.GetValue(template)?.ToString(),
                        Description = template.GetType().GetProperty($"CustomString{i}Description")?.GetValue(template)?.ToString(),
                        IsRequired = (bool)(template.GetType().GetProperty($"CustomString{i}Required")?.GetValue(template) ?? false),
                        Order = order++
                    });
                }
            }

            // Text questions
            for (int i = 1; i <= 4; i++)
            {
                if ((bool)template.GetType().GetProperty($"CustomText{i}State")?.GetValue(template))
                {
                    questions.Add(new QuestionViewModel
                    {
                        Type = "text",
                        FieldName = $"CustomText{i}Answer",
                        Question = template.GetType().GetProperty($"CustomText{i}Question")?.GetValue(template)?.ToString(),
                        Description = template.GetType().GetProperty($"CustomText{i}Description")?.GetValue(template)?.ToString(),
                        IsRequired = (bool)(template.GetType().GetProperty($"CustomText{i}Required")?.GetValue(template) ?? false),
                        Order = order++
                    });
                }
            }

            // Integer questions
            for (int i = 1; i <= 4; i++)
            {
                if ((bool)template.GetType().GetProperty($"CustomInt{i}State")?.GetValue(template))
                {
                    questions.Add(new QuestionViewModel
                    {
                        Type = "integer",
                        FieldName = $"CustomInt{i}Answer",
                        Question = template.GetType().GetProperty($"CustomInt{i}Question")?.GetValue(template)?.ToString(),
                        Description = template.GetType().GetProperty($"CustomInt{i}Description")?.GetValue(template)?.ToString(),
                        IsRequired = (bool)(template.GetType().GetProperty($"CustomInt{i}Required")?.GetValue(template) ?? false),
                        Order = order++
                    });
                }
            }

            // Checkbox questions
            for (int i = 1; i <= 4; i++)
            {
                if ((bool)template.GetType().GetProperty($"CustomCheckbox{i}State")?.GetValue(template))
                {
                    questions.Add(new QuestionViewModel
                    {
                        Type = "checkbox",
                        FieldName = $"CustomCheckbox{i}Answer",
                        Question = template.GetType().GetProperty($"CustomCheckbox{i}Question")?.GetValue(template)?.ToString(),
                        Description = template.GetType().GetProperty($"CustomCheckbox{i}Description")?.GetValue(template)?.ToString(),
                        IsRequired = (bool)(template.GetType().GetProperty($"CustomCheckbox{i}Required")?.GetValue(template) ?? false),
                        Order = order++
                    });
                }
            }

            return questions.OrderBy(q => q.Order).ToList();
        }

        private Dictionary<string, string> GetDisplayFields(Form form)
        {
            var fields = new Dictionary<string, string>();
            var template = form.Template;

            // Check ShowInTable for each field type
            for (int i = 1; i <= 4; i++)
            {
                // String fields
                if ((bool)(template.GetType().GetProperty($"CustomString{i}ShowInTable")?.GetValue(template) ?? false))
                {
                    var question = template.GetType().GetProperty($"CustomString{i}Question")?.GetValue(template)?.ToString();
                    var answer = form.GetType().GetProperty($"CustomString{i}Answer")?.GetValue(form)?.ToString();
                    if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                        fields[question] = answer;
                }

                // Text fields
                if ((bool)(template.GetType().GetProperty($"CustomText{i}ShowInTable")?.GetValue(template) ?? false))
                {
                    var question = template.GetType().GetProperty($"CustomText{i}Question")?.GetValue(template)?.ToString();
                    var answer = form.GetType().GetProperty($"CustomText{i}Answer")?.GetValue(form)?.ToString();
                    if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                        fields[question] = answer.Length > 50 ? answer.Substring(0, 50) + "..." : answer;
                }

                // Integer fields
                if ((bool)(template.GetType().GetProperty($"CustomInt{i}ShowInTable")?.GetValue(template) ?? false))
                {
                    var question = template.GetType().GetProperty($"CustomInt{i}Question")?.GetValue(template)?.ToString();
                    var answer = form.GetType().GetProperty($"CustomInt{i}Answer")?.GetValue(form);
                    if (!string.IsNullOrEmpty(question) && answer != null)
                        fields[question] = answer.ToString();
                }

                // Checkbox fields
                if ((bool)(template.GetType().GetProperty($"CustomCheckbox{i}ShowInTable")?.GetValue(template) ?? false))
                {
                    var question = template.GetType().GetProperty($"CustomCheckbox{i}Question")?.GetValue(template)?.ToString();
                    var answer = (bool)(form.GetType().GetProperty($"CustomCheckbox{i}Answer")?.GetValue(form) ?? false);
                    if (!string.IsNullOrEmpty(question))
                        fields[question] = answer ? "Yes" : "No";
                }
            }

            return fields;
        }

        private List<AnswerViewModel> GetFormAnswers(Form form, Template template)
        {
            var answers = new List<AnswerViewModel>();

            // Process all active questions
            var questions = GetActiveQuestions(template);
            
            foreach (var question in questions)
            {
                var answer = form.GetType().GetProperty(question.FieldName)?.GetValue(form);
                
                answers.Add(new AnswerViewModel
                {
                    Question = question.Question,
                    Answer = answer?.ToString() ?? "",
                    Type = question.Type
                });
            }

            return answers;
        }
    }
}