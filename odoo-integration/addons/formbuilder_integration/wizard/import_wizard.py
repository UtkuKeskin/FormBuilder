# -*- coding: utf-8 -*-
from odoo import models, fields, api
from odoo.exceptions import UserError
import requests
import json

class FormBuilderImportWizard(models.TransientModel):
    _name = 'formbuilder.import.wizard'
    _description = 'FormBuilder Import Wizard'
    
    api_url = fields.Char(string='API URL', required=True, 
                         default='http://host.docker.internal:5175/api/v1/templates/aggregates')
    api_key = fields.Char(string='API Key', required=True,
                         help='Enter your FormBuilder API key (starts with FB_)')
    
    # Step management
    state = fields.Selection([
        ('credentials', 'API Credentials'),
        ('preview', 'Preview Data'),
        ('done', 'Import Complete')
    ], default='credentials', string='State')
    
    # Preview data
    preview_data = fields.Text(string='Preview Data')
    template_count = fields.Integer(string='Templates to Import', readonly=True)
    
    def action_test_connection(self):
        """Test API connection"""
        self.ensure_one()
        
        if not self.api_key.startswith('FB_'):
            raise UserError('Invalid API key format. API key should start with "FB_"')
        
        try:
            headers = {'X-API-Key': self.api_key}
            response = requests.get(self.api_url, headers=headers, timeout=10)
            
            if response.status_code == 401:
                raise UserError('Invalid API key. Please check your credentials.')
            elif response.status_code == 429:
                raise UserError('Rate limit exceeded. Please try again later.')
            elif response.status_code != 200:
                raise UserError(f'API error: {response.status_code} - {response.text}')
            
            data = response.json()
            templates = data.get('templates', [])
            
            # Store preview data
            self.preview_data = json.dumps(templates, indent=2)
            self.template_count = len(templates)
            self.state = 'preview'
            
            return {
                'type': 'ir.actions.act_window',
                'res_model': self._name,
                'view_mode': 'form',
                'res_id': self.id,
                'target': 'new',
            }
            
        except requests.exceptions.Timeout:
            raise UserError('Connection timeout. Please check the API URL.')
        except requests.exceptions.ConnectionError:
            raise UserError('Cannot connect to API. Please check the URL and your network.')
        except Exception as e:
            raise UserError(f'Error: {str(e)}')
    
    def action_import(self):
        """Import templates from API"""
        self.ensure_one()
        
        if not self.preview_data:
            raise UserError('No data to import. Please test connection first.')
        
        try:
            templates_data = json.loads(self.preview_data)
            imported_count = 0
            
            for template_data in templates_data:
                external_id = template_data.get('id')
                
                # Check if template already exists
                existing_template = self.env['formbuilder.template'].search([
                    ('external_id', '=', external_id),
                    ('api_url', '=', self.api_url)
                ], limit=1)
                
                if existing_template:
                    # Update existing
                    existing_template.sync_from_api(self.api_key, template_data)
                else:
                    # Create new
                    new_template = self.env['formbuilder.template'].create({
                        'name': template_data.get('title', 'Untitled'),
                        'author': template_data.get('author', 'Unknown'),
                        'external_id': external_id,
                        'api_url': self.api_url,
                    })
                    new_template.sync_from_api(self.api_key, template_data)
                
                imported_count += 1
            
            self.state = 'done'
            
            # Show success message
            message = f'Successfully imported {imported_count} template(s)'
            
            return {
                'type': 'ir.actions.client',
                'tag': 'display_notification',
                'params': {
                    'title': 'Import Complete',
                    'message': message,
                    'type': 'success',
                    'sticky': False,
                }
            }
            
        except Exception as e:
            raise UserError(f'Import error: {str(e)}')
    
    def action_view_templates(self):
        """View imported templates"""
        return {
            'type': 'ir.actions.act_window',
            'res_model': 'formbuilder.template',
            'view_mode': 'tree,form',
            'target': 'current',
        }