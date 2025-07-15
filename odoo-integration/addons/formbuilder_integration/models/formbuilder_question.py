# -*- coding: utf-8 -*-
from odoo import models, fields, api
import json

class FormBuilderQuestion(models.Model):
    _name = 'formbuilder.question'
    _description = 'FormBuilder Question'
    _order = 'template_id, id'
    
    template_id = fields.Many2one('formbuilder.template', string='Template', required=True, ondelete='cascade')
    text = fields.Char(string='Question Text', required=True)
    type = fields.Selection([
        ('string', 'Short Text'),
        ('text', 'Long Text'),
        ('integer', 'Number'),
        ('checkbox', 'Checkbox')
    ], string='Question Type', required=True, default='string')
    answer_count = fields.Integer(string='Number of Answers', default=0)
    aggregation_data = fields.Text(string='Aggregation Data (JSON)')
    
    # Computed fields for display
    aggregation_display = fields.Html(string='Aggregation Summary', compute='_compute_aggregation_display')
    type_color = fields.Char(string='Type Color', compute='_compute_type_color')
    
    @api.depends('aggregation_data', 'type')
    def _compute_aggregation_display(self):
        for record in self:
            if not record.aggregation_data:
                record.aggregation_display = '<p>No data available</p>'
                continue
                
            try:
                data = json.loads(record.aggregation_data)
                html = '<div class="formbuilder-aggregation">'
                
                if record.type == 'integer':
                    avg = data.get('average', 0)
                    min_val = data.get('min', 0)
                    max_val = data.get('max', 0)
                    html += f'''
                        <div class="row">
                            <div class="col-4 text-center">
                                <strong>Average</strong><br/>
                                <span class="badge badge-primary">{avg}</span>
                            </div>
                            <div class="col-4 text-center">
                                <strong>Min</strong><br/>
                                <span class="badge badge-info">{min_val}</span>
                            </div>
                            <div class="col-4 text-center">
                                <strong>Max</strong><br/>
                                <span class="badge badge-warning">{max_val}</span>
                            </div>
                        </div>
                    '''
                    
                elif record.type in ['string', 'text']:
                    top_answers = data.get('topAnswers', [])
                    if top_answers:
                        html += '<strong>Top Answers:</strong><ul>'
                        for answer in top_answers[:5]:
                            html += f'<li>{answer}</li>'
                        html += '</ul>'
                    else:
                        html += '<p>No answers yet</p>'
                        
                elif record.type == 'checkbox':
                    true_pct = data.get('truePercentage', 0)
                    false_pct = data.get('falsePercentage', 0)
                    html += f'''
                        <div class="progress" style="height: 25px;">
                            <div class="progress-bar bg-success" style="width: {true_pct}%">
                                Yes ({true_pct}%)
                            </div>
                            <div class="progress-bar bg-danger" style="width: {false_pct}%">
                                No ({false_pct}%)
                            </div>
                        </div>
                    '''
                    
                html += '</div>'
                record.aggregation_display = html
                
            except Exception as e:
                record.aggregation_display = f'<p class="text-danger">Error parsing data: {str(e)}</p>'
    
    @api.depends('type')
    def _compute_type_color(self):
        color_map = {
            'string': 'primary',
            'text': 'success',
            'integer': 'warning',
            'checkbox': 'info'
        }
        for record in self:
            record.type_color = color_map.get(record.type, 'secondary')