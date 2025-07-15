using System.Collections.Generic;

namespace FormBuilder.Core.Models.Api
{
    public class TemplateAggregateResponse
    {
        public List<TemplateAggregate> Templates { get; set; } = new();
    }

    public class TemplateAggregate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public List<QuestionAggregate> Questions { get; set; } = new();
    }

    public class QuestionAggregate
    {
        public string Text { get; set; }
        public string Type { get; set; }
        public int AnswerCount { get; set; }
        public AggregationData Aggregation { get; set; }
    }

    public class AggregationData
    {
        // For numeric types
        public double? Average { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        
        // For text types
        public List<string> TopAnswers { get; set; }
        
        // For checkbox
        public double? TruePercentage { get; set; }
        public double? FalsePercentage { get; set; }
    }
}