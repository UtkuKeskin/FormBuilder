# -*- coding: utf-8 -*-
{
    'name': 'FormBuilder Integration',
    'version': '16.0.1.0.0',
    'category': 'Tools',
    'summary': 'Integration with FormBuilder application for template data import',
    'description': """
FormBuilder Integration Module
==============================

This module allows you to:
- Import template data from FormBuilder API
- View aggregated results for forms
- Display statistics for different question types
- Read-only access to FormBuilder data

Features:
---------
* API Token based authentication
* Template and question data import
* Aggregated statistics display
* Automatic data synchronization
    """,
    'author': 'FormBuilder Team',
    'website': 'https://formbuilder.com',
    'depends': ['base', 'web'],
    'data': [
        'security/security.xml',
        'security/ir.model.access.csv',
        'views/menu_views.xml',
        'views/template_views.xml',
        'wizard/import_wizard_view.xml',
        'data/default_data.xml',
    ],
    'license': 'LGPL-3',
    'installable': True,
    'application': True,
    'auto_install': False,
}