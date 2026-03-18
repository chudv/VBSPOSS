CKEDITOR.plugins.add('ifButton', {
    icons: 'ifButton', // CKEditor sẽ tự load từ /icons/ifButton.png
    init: function (editor) {
        editor.addCommand('insertIfBlock', {
            exec: function (editor) {
                var template = "<#if>\n\t\n</#if>";
                editor.insertHtml(template);
            }
        });
        editor.ui.addButton('IfButton', {
            label: 'Insert IF block',
            command: 'insertIfBlock'
        });
    }
});