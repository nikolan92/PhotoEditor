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
            undoStack = new InfinityStack<ICommand>(historySize);
            redoStack = new InfinityStack<ICommand>(historySize);
        }
        
        public void AddToHistory(ICommand command, WriteableBitmap imageData)
        {
            command.Execute(imageData);
            undoStack.Push(command);
            redoStack.Clear();
        }
        public void Undo(WriteableBitmap imageData)
        {
            try
            {
                ICommand undoCommand = undoStack.Pop();
                undoCommand.UnExecute(imageData);
                redoStack.Push(undoCommand);
            } catch(InvalidOperationException e)
            {
                Console.WriteLine(e.ToString());
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
