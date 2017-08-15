using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;

namespace NeuroSoft.DeviceAutoTest.TestTemplateEditor
{
    /// <summary>
    /// 
    /// </summary>
    public class DATRichTextBox : RichTextBox
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public DATRichTextBox()
            : base()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnExecutedPasteCommand, OnCanExecutePasteCommand));            
        }

        //Paste
        private void OnCanExecutePasteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OnExecutedPasteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Paste();
        }

        /// <summary>
        /// Вставка
        /// </summary>
        public new void Paste()
        {
            startPasting();
            try
            {
                BitmapSource imageSource = Clipboard.GetImage();
                if (imageSource != null && string.IsNullOrEmpty(Selection.Start.GetTextInRun(LogicalDirection.Backward)) &&
                    string.IsNullOrEmpty(Selection.End.GetTextInRun(LogicalDirection.Forward)))
                {
                    Selection.Text = " ";
                    var selStart = Selection.Start;
                    Selection.Select(Selection.End, Selection.End);
                    base.Paste();
                    selStart.DeleteTextInRun(1);                    
                    //    Image image = new Image();
                    //    image.BeginInit();
                    //    image.Source = imageSource;
                    //    image.EndInit();
                    //    image.Stretch = Stretch.Uniform;
                    //    image.Width = imageSource.Width;
                    //    image.Height = imageSource.Height;
                    //    Selection.Text = "";
                    //    new InlineUIContainer(image, Selection.Start);
                    //    return;
                }
                else
                {
                    base.Paste();
                }
            }
            finally
            {
                endPasting();
            }
        }
        
        private bool pastingInProcess = false;
        /// <summary>
        /// Зафиксировать начало вставки
        /// </summary>
        private void startPasting()
        {
            pastingInProcess = true;
        }
        /// <summary>
        /// Зафиксировать окончание вставки
        /// </summary>
        private void endPasting()
        {
            pastingInProcess = false;
        }
        
        /// <summary>
        /// Событие изменения текста
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (pastingInProcess || e.UndoAction == UndoAction.Redo || e.UndoAction == UndoAction.Undo)
            {
                var start = Document.ContentStart;
                foreach (var change in e.Changes)
                {
                    if (change.AddedLength > 0)
                    {
                        var startPos = start.GetPositionAtOffset(change.Offset);
                        var endPos = start.GetPositionAtOffset(change.Offset + change.AddedLength);
                        var textRange = new TextRange(startPos, endPos);
                        AddResizeAdornerToImagesInTextRange(textRange);                        
                    }
                }
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (lastSelectedImage != null && e.OriginalSource != lastSelectedImage && 
                (e.OriginalSource is Image || e.OriginalSource is TextElement))
            {
                NeuroSoft.WPFComponents.ProtocolPatternMaker.ProtocolPatternMakerHelper.RemoveResizeAdorner(lastSelectedImage);
                lastSelectedImage = null;
            }
        }


        /// <summary>
        /// Добавление ResizingAdorner к изображениям внутри textRange
        /// </summary>
        /// <param name="textRange"></param>
        public void AddResizeAdornerToImagesInTextRange(TextRange textRange)
        {
            IEnumerable<Image> images = NeuroSoft.WPFComponents.ProtocolPatternMaker.ProtocolPatternMakerHelper.ExtractFromTextRange<Image>(textRange);
            if (images != null)
            {
                foreach (var image in images)
                {
                    AddResizeAdornerToImage(image);
                }
            }
        }

        private Image lastSelectedImage = null;

        /// <summary>
        /// Добавление ResizingAdorner к изображению
        /// </summary>
        /// <param name="image"></param>
        public void AddResizeAdornerToImage(Image image)
        {
            if (image != null)
            {
                image.Stretch = Stretch.Fill;
                image.MouseLeftButtonDown += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) => 
                {
                    NeuroSoft.WPFComponents.ProtocolPatternMaker.ProtocolPatternMakerHelper.AddResizeAdorner(image);
                    lastSelectedImage = image;
                });
            }
        }

        public void SetDocument(FlowDocument document)
        {
            Document = document;
            if (document.IsLoaded)
            {
                AddResizeAdornerToImagesInTextRange(new TextRange(Document.ContentStart, Document.ContentEnd));
            }
            else
            {
                document.Loaded += new RoutedEventHandler((object sender, RoutedEventArgs e) =>
                {
                    AddResizeAdornerToImagesInTextRange(new TextRange(Document.ContentStart, Document.ContentEnd));
                });                
            }
        }
    }
}
