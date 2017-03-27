using PhotoEditor.DataModel;
using PhotoEditor.Utility;
using System;

namespace PhotoEditor.HistoryProvider
{
    public class HistoryHelper
    {
        InfinityStack<ImageReferenceKeeper> undoStack;
        InfinityStack<ImageReferenceKeeper> redoStack;
        public HistoryHelper(int historySize)
        {
            undoStack = new InfinityStack<ImageReferenceKeeper>(historySize, null);
            redoStack = new InfinityStack<ImageReferenceKeeper>(historySize, null);
        }

        public void Archive(ImageModel imageModel)
        {
            undoStack.Push(new ImageReferenceKeeper(imageModel));
            redoStack.Clear();
        }
        public void Undo(ImageModel imageModel)
        {
            try
            {
                ImageReferenceKeeper undoImage = undoStack.Pop();
                undoImage.SwapReference(imageModel);
                redoStack.Push(undoImage);
            } catch(InvalidOperationException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void Redo(ImageModel imageModel)
        {
            try
            {
                ImageReferenceKeeper redoImage = redoStack.Pop();
                redoImage.SwapReference(imageModel);
                undoStack.Push(redoImage);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public bool CanUndo()
        {
            return undoStack.Count != 0;
        }
        public bool CanRedo()
        {
            return redoStack.Count != 0;
        }
    }
}


//public void ArchiveAndExecute(ICommand command, WriteableBitmap imageData)
//{
//    command.Execute(imageData);
//    undoStack.Push(command);
//    redoStack.Clear();
//}