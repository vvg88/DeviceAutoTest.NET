using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.WPFComponents.CommandManager;

namespace NeuroSoft.DeviceAutoTest.Commands
{
    /// <summary>
    /// Команды редактирования документа
    /// </summary>
    public static class DATEditingCommands
    {
        /// <summary>
        /// Имя группы
        /// </summary>
        public const string Group = "DATEditingCommandsGroup";
        /// <summary>
        /// Команда изменения фонового цвета в редакторе
        /// </summary>
        public static readonly NSCommand EditBackColorCommand = new NSCommand("EditBackColorCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда изменения цвета шрифта в редакторе
        /// </summary>
        public static readonly NSCommand EditColorCommand = new NSCommand("EditColorCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Жирный
        /// </summary>
        public static readonly NSCommand ToggleBoldCommand = new NSCommand("ToogleBoldCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Курсив
        /// </summary>
        public static readonly NSCommand ToggleItalicCommand = new NSCommand("ToogleItalicCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Подчеркнутый
        /// </summary>
        public static readonly NSCommand ToggleUnderlineCommand = new NSCommand("ToogleUnderlineCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Маркированный список
        /// </summary>
        public static readonly NSCommand ToggleBulletsCommand = new NSCommand("ToggleBulletsCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда выравнивания текста в редакторе по центру
        /// </summary>
        public static readonly NSCommand FormatCenterAlignCommand = new NSCommand("FormatCenterAlignCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда выравнивания текста в редакторе по левому краю
        /// </summary>
        public static readonly NSCommand FormatLeftAlignCommand = new NSCommand("FormatLeftAlignCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда выравнивания текста в редакторе по правому краю
        /// </summary>
        public static readonly NSCommand FormatRightAlignCommand = new NSCommand("FormatRightAlignCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Изменение шрифта в редакторе
        /// </summary>
        public static readonly NSCommand FormatFontCommand = new NSCommand("FormatFontCommand", typeof(DATTemplateCommands));
    }

    /// <summary>
    /// Команды действий над сценарием тестирования
    /// </summary>
    public static class DATTemplateCommands
    {
        /// <summary>
        /// Имя группы
        /// </summary>
        public const string Group = "DATTemplateCommandsGroup";
        /// <summary>
        /// Команда создания новой папки в дереве тестов
        /// </summary>
        public static readonly NSCommand CreateDATFolderCommand = new NSCommand("CreateDATFolderCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда добавления нового действия
        /// </summary>
        public static readonly NSCommand AddTestCommand = new NSCommand("AddTestCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда переименования сценария тестирования
        /// </summary>
        public static readonly NSCommand RenameTemplateCommand = new NSCommand("RenameTemplateCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда экспортирования сценария тестирования в файл
        /// </summary>
        public static readonly NSCommand ExportTemplateCommand = new NSCommand("ExportTemplateCommand", typeof(DATTemplateCommands)/*, true*/);

        /// <summary>
        /// Команда перемещения узла вверх
        /// </summary>
        public static readonly NSCommand MoveUpNodeCommand = new NSCommand("MoveUpNodeCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда перемещения узла вниз
        /// </summary>
        public static readonly NSCommand MoveDownNodeCommand = new NSCommand("MoveDownNodeCommand", typeof(DATTemplateCommands));

        /// <summary>
        /// Команда удаления элемента дерева
        /// </summary>
        public static readonly NSCommand RemoveNodeCommand = new NSCommand("RemoveNodeCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда переименование узла в дереве тестов шаблона
        /// </summary>
        public static readonly NSCommand RenameNodeCommand = new NSCommand("RenameNodeCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда изменения идентификатора теста
        /// </summary>
        public static readonly NSCommand ChangeTestIdCommand = new NSCommand("ChangeTestIdCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда сохранения сценария тестирования
        /// </summary>
        public static readonly NSCommand SaveTemplateCommand = new NSCommand("SaveTemplateCommand", typeof(DATTemplateCommands));        
        /// <summary>
        /// Команда открытия теста шаблона
        /// </summary>
        public static readonly NSCommand OpenTestCommand = new NSCommand("OpenTestCommand", typeof(DATTemplateCommands));

        /// <summary>
        /// Команда редактирования шаблонов тестов
        /// </summary>
        public static readonly NSCommand OpenTestTemplateCommand = new NSCommand("OpenTestTemplateCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда редактирования используемой в данный момент инструкции по наладке
        /// </summary>
        public static readonly NSCommand EditUsedTestTemplateCommand = new NSCommand("EditUsedTestTemplateCommand", typeof(DATTemplateCommands));

        /// <summary>
        /// Редактирование списка переменных шаблона
        /// </summary>
        public static readonly NSCommand EditDATVariablesCommand = new NSCommand("EditDATVariablesCommand", typeof(DATTemplateCommands)/*, true*/);
        /// <summary>
        /// Редактирование списка групп действий
        /// </summary>
        public static readonly NSCommand EditTestGroupsCommand = new NSCommand("EditTestGroupsCommand", typeof(DATTemplateCommands)/*, true*/);
        /// <summary>
        /// Редактирование списка используемых в тесте переменных
        /// </summary>
        public static readonly NSCommand EditUsedInTestVariablesListCommand = new NSCommand("EditUsedInTestVariablesListCommand", typeof(DATTemplateCommands));

        /// <summary>
        /// Команда предварительного просмотра описания теста
        /// </summary>
        public static readonly NSCommand PreviewCommand = new NSCommand("PreviewCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда редактирования условий выполнения действия
        /// </summary>
        public static readonly NSCommand EditValidationCommand = new NSCommand("EditValidationCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда редактирования скрипта, выполняемого при открытии теста
        /// </summary>
        public static readonly NSCommand EditBeginTestScriptCommand = new NSCommand("EditBeginTestScriptCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда редактирования скрипта, выполняемого при закрыти теста
        /// </summary>
        public static readonly NSCommand EditEndTestScriptCommand = new NSCommand("EditEndTestScriptCommand", typeof(DATTemplateCommands)/*, true*/);                

        /// <summary>
        /// Команда выбора теста, после которого следует запускать текущий тест
        /// </summary>
        public static readonly NSCommand SelectPreviousTestCommand = new NSCommand("SelectPreviousTestCommand", typeof(DATTemplateCommands)/*, true*/);
        /// <summary>
        /// Команда редактирования списка используемых в скрипте теста пространств имен
        /// </summary>
        public static readonly NSCommand TestScriptUsingsCommand = new NSCommand("TestScriptUsingsCommand", typeof(DATTemplateCommands)/*, true*/);
        /// <summary>
        /// Команда настройки списка возможных ссылок на содержимое в тесте
        /// </summary>
        public static readonly NSCommand ContentPresenterTagsCommand = new NSCommand("ContentPresenterTagsCommand", typeof(DATTemplateCommands)/*, true*/);
        /// <summary>
        /// Команда настройки списка скриптов-кнопок
        /// </summary>
        public static readonly NSCommand ButtonScriptsCommand = new NSCommand("ButtonScriptsCommand", typeof(DATTemplateCommands)/*, true*/);
        /// <summary>
        /// Команда настройки теста при автоматическом тестировании
        /// </summary>
        public static readonly NSCommand AutoTestSettingsCommand = new NSCommand("AutoTestSettingsCommand", typeof(DATTemplateCommands)/*, true*/);

        /// <summary>
        /// Команда редактирования шаблонов протоколов инструкции по наладке
        /// </summary>
        public static readonly NSCommand ProtocolPatternsCommand = new NSCommand("ProtocolPatternsCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда валидации всех скриптов инструкции
        /// </summary>
        public static readonly NSCommand ValidateScriptsCommand = new NSCommand("ValidateScriptsCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда смены типа действия (составное/несоставное)
        /// </summary>
        public static readonly NSCommand ChangeIsContainerCommand = new NSCommand("ChangeIsContainerCommand", typeof(DATTemplateCommands));

        /// <summary>
        /// Команда вставки изображения в редактор из файла
        /// </summary>
        public static readonly NSCommand PasteImageLinkCommand = new NSCommand("PasteImageLinkCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда открытия папки с прикрепленными к инструкции изображениями
        /// </summary>
        public static readonly NSCommand OpenImagesFolderCommand = new NSCommand("OpenImagesFolderCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда изменения версии инструкции
        /// </summary>
        public static readonly NSCommand ChangeTemplateVersionCommand = new NSCommand("ChangeTemplateVersionCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда редактирования списка возможных исправлений в ходе теста
        /// </summary>
        public static readonly NSCommand EditTestCorrectionsCommand = new NSCommand("EditTestCorrectionsCommand", typeof(DATTemplateCommands)/*, true*/);        
        /// <summary>
        /// Команда сохранения инструкции в текущей базе
        /// </summary>
        public static readonly NSCommand SaveTemplateInDBCommand = new NSCommand("SaveTemplateInDBCommand", typeof(DATTemplateCommands));
        /// <summary>
        /// Команда изменения картотеки по умолчанию
        /// </summary>
        public static readonly NSCommand EditDefaultCardCommand = new NSCommand("EditDefaultCardCommand", typeof(DATTemplateCommands)/*, true*/);
    }

    /// <summary>
    /// Команды в процессе тестирования
    /// </summary>
    public static class DATTestingCommands
    {
        /// <summary>
        /// Имя группы
        /// </summary>
        public const string Group = "DATTestingCommandsGroup";
        /// <summary>
        /// Команда начала/продолжения тестирования устройства
        /// </summary>
        public static readonly NSCommand StartTestingCommand = new NSCommand("StartTestingCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда просмотра истории тестирования
        /// </summary>
        public static readonly NSCommand ShowTestingHistoryCommand = new NSCommand("ShowTestingHistoryCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда просмотра истории по одному действию
        /// </summary>
        public static readonly NSCommand ShowTestHistoryCommand = new NSCommand("ShowTestHistoryCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда открытия теста в процессе тестирования
        /// </summary>
        public static readonly NSCommand OpenTestCommand = new NSCommand("OpenTestCommand", typeof(DATTestingCommands));        
        /// <summary>
        /// Команда открытия следующего теста
        /// </summary>
        public static readonly NSCommand NextCommand = new NSCommand("NextCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда открытия предыдущего теста
        /// </summary>
        public static readonly NSCommand PreviousCommand = new NSCommand("PreviousCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда обновления действия
        /// </summary>
        public static readonly NSCommand RefreshTestCommand = new NSCommand("RefreshTestCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда остановки действия
        /// </summary>
        public static readonly NSCommand StopTestCommand = new NSCommand("StopTestCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда старта действия
        /// </summary>
        public static readonly NSCommand StartTestCommand = new NSCommand("StartTestCommand", typeof(DATTestingCommands));

        /// <summary>
        /// Команда старта автоматического тестирования
        /// </summary>
        public static readonly NSCommand StartAutoTestCommand = new NSCommand("StartAutoTestCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда завершения автоматического тестирования
        /// </summary>
        public static readonly NSCommand StopAutoTestCommand = new NSCommand("StopAutoTestCommand", typeof(DATTestingCommands));

        /// <summary>
        /// Команда выполнения действия
        /// </summary>
        public static readonly NSCommand ExecuteTestCommand = new NSCommand("ExecuteTestCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда успешного выполнения действия
        /// </summary>
        public static readonly NSCommand SuccessTestCommand = new NSCommand("SuccessTestCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда выполнения действия с ошибками
        /// </summary>
        public static readonly NSCommand HasErrorsTestCommand = new NSCommand("HasErrorsTestCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда выполнения действия с исправлениями
        /// </summary>
        public static readonly NSCommand CorrectTestCommand = new NSCommand("CorrectTestCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда повторного прохождения теста
        /// </summary>
        public static readonly NSCommand ReTestCommand = new NSCommand("ReTestCommand", typeof(DATTestingCommands));
        /// <summary>
        /// Команда подтверждения завершения теста
        /// </summary>
        public static readonly NSCommand ConfirmFinishTestCommand = new NSCommand("ConfirmFinishTestCommand", typeof(DATTestingCommands));        

        /// <summary>
        /// Команда нажатия кнопки 1 на педали
        /// </summary>
        public static readonly NSCommand PressButton1Command = new NSCommand("PressButton1Command", typeof(DATTestingCommands));
        /// <summary>
        /// Команда нажатия кнопки 2 на педали
        /// </summary>
        public static readonly NSCommand PressButton2Command = new NSCommand("PressButton2Command", typeof(DATTestingCommands));
        /// <summary>
        /// Команда нажатия кнопки 3 на педали
        /// </summary>
        public static readonly NSCommand PressButton3Command = new NSCommand("PressButton3Command", typeof(DATTestingCommands));
    }

    /// <summary>
    /// Команды настроек
    /// </summary>
    public static class DATSettingsCommands
    {
        /// <summary>
        /// Имя группы
        /// </summary>
        public const string Group = "DATSettingsCommandsGroup";
        /// <summary>
        /// 
        /// </summary>
        public static readonly NSCommand ToggleAutoSaveCommand = new NSCommand("ToggleAutoSaveCommand", typeof(DATSettingsCommands));
    }
}
