using PhotoEditor.HistoryProvider.Commands;
using PhotoEditor.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.HistoryProvider
{
    public class HistoryHelper
    {
        InfinityStack<ICommand> undoStack;
        InfinityStack<ICommand> redoStack;
        public HistoryHelper(int historySize)
        {
            undoStack = new InfinityStack<ICommand>(historySize, null);
            redoStack = new InfinityStack<ICommand>(historySize, null);
        }
        

        public void Archive(ICommand command)
        {
            undoStack.Push(command);
            redoStack.Clear();
        }
        public WriteableBitmap Undo(WriteableBitmap imageData)
        {
            WriteableBitmap undoImage = null;
            try
            {
                ICommand undoCommand = undoStack.Pop();
                undoImage = undoCommand.UnExecute(imageData);
                redoStack.Push(undoCommand);
                return undoImage;
            } catch(InvalidOperationException e)
            {
                Console.WriteLine(e.ToString());
                return undoImage;
            }
        }
        public void Redo(WriteableBitmap imageData)
        {
            try
            {
                ICommand redoCommand = redoStack.Pop();
                redoCommand.Execute(imageData);
                undoStack.Push(redoCommand);
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