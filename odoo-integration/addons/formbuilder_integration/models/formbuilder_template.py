# -*- coding: utf-8 -*-
from odoo import models, fields, api
import json
import requests
from datetime import datetime

class FormBuilderTemplate(models.Model):
    _name = 'formbuilder.template'
    _description = 'FormBuilder Template'
    _order = 'create_date desc'
    
    name = fields.Char(string='Template Name', required=True)
    author = fields.Char(string='Author Email', required=True)
    external_id = fields.Integer(string='FormBuilder ID', required=True)
    api_url = fields.Char(string='API URL', required=True)
    last_sync = fields.Datetime(string='Last Synchronization')
    
    # Relations
    question_ids = fields.One2many('formbuilder.question', 'template_id', string='Questions')
    
    # Computed fields
    question_count = fields.Integer(string='Number of Questions', compute='_compute_question_count', store=True)
    total_responses = fields.Integer(string='Total Responses', compute='_compute_total_responses', store=True)
    
    # Constraints
    _sql_constraints = [
        ('unique_external_api', 'unique(external_id, api_url)', 
         'A template with this ID already exists for this API URL!')
    ]
    
    @api.depends('question_ids')
    def _compute_question_count(self):
        for record in self:
            record.question_count = len(record.question_ids)
    
    @api.depends('question_ids.answer_count')
    def _compute_total_responses(self):
        for record in self:
            # Get the maximum answer count from any question
            counts = record.question_ids.mapped('answer_count')
            record.total_responses = max(counts) if counts else 0
    
    def sync_from_api(self, api_key, template_data):
        """Sync template data from API response"""
        self.ensure_one()
        
        # Update template fields
        self.write({
            'name': template_data.get('title', self.name),
            'author': template_data.get('author', self.author),
            'last_sync': fields.Datetime.now(),
        })
        
        # Sync questions
        existing_questions = {q.text: q for q in self.question_ids}
        
        for question_data in template_data.get('questions', []):
            question_text = question_data.get('text', '')
            
            if question_text in existing_questions:
                # Update existing question
                existing_questions[question_text].write({
                    'type': question_data.get('type', 'string'),
                    'answer_count': question_data.get('answerCount', 0),
                    'aggregation_data': json.dumps(question_data.get('aggregation', {})),
                })
            else:
                # Create new question
                self.env['formbuilder.question'].create({
                    'template_id': self.id,
                    'text': question_text,
                    'type': question_data.get('type', 'string'),
                    'answer_count': question_data.get('answerCount', 0),
                    'aggregation_data': json.dumps(question_data.get('aggregation', {})),
                })
        
        return True
    
    def action_view_questions(self):
        """Action to view questions"""
        self.ensure_one()
        return {
            'name': 'Questions',
            'type': 'ir.actions.act_window',
            'res_model': 'formbuilder.question',
            'view_mode': 'tree,form',
            'domain': [('template_id', '=', self.id)],
            'context': {'default_template_id': self.id}
        }