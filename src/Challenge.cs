using System;
using System.Collections.Generic;

namespace DesignPatternChallenge
{
    // ============================
    // 1) RECEIVER (quem executa de verdade)
    // ============================
    public class TextEditor
    {
        private string _content = "";
        private int _cursorPosition = 0;

        public void InsertText(string text)
        {
            _content = _content.Insert(_cursorPosition, text);
            _cursorPosition += text.Length;

            Console.WriteLine($"[Editor] Texto inserido: '{text}'");
            Console.WriteLine($"[Editor] Conteúdo atual: '{_content}'");
        }

        public string DeleteText(int length)
        {
            if (length <= 0) return "";
            if (_cursorPosition < length) length = _cursorPosition;

            var start = _cursorPosition - length;
            var removed = _content.Substring(start, length);

            _content = _content.Remove(start, length);
            _cursorPosition -= length;

            Console.WriteLine($"[Editor] {length} caracteres deletados");
            Console.WriteLine($"[Editor] Conteúdo atual: '{_content}'");

            return removed;
        }

        public void SetBold(int start, int length)
        {
            Console.WriteLine($"[Editor] Aplicando negrito de {start} a {start + length}");
            // Simulação
        }

        public void RemoveBold(int start, int length)
        {
            Console.WriteLine($"[Editor] Removendo negrito de {start} a {start + length}");
            // Simulação
        }

        public void SetCursorPosition(int position)
        {
            if (position < 0) position = 0;
            if (position > _content.Length) position = _content.Length;
            _cursorPosition = position;
        }

        public int GetCursorPosition() => _cursorPosition;
        public string GetContent() => _content;
    }

    // ============================
    // 2) COMMAND
    // ============================
    public interface IEditorCommand
    {
        void Execute();
        void Undo();
    }

    // ============================
    // 3) CONCRETE COMMANDS
    // ============================
    public class InsertTextCommand : IEditorCommand
    {
        private readonly TextEditor _editor;
        private readonly string _text;
        private int _positionBefore;

        public InsertTextCommand(TextEditor editor, string text)
        {
            _editor = editor;
            _text = text;
        }

        public void Execute()
        {
            _positionBefore = _editor.GetCursorPosition();
            _editor.InsertText(_text);
        }

        public void Undo()
        {
            // desfazer insert = deletar o que foi inserido
            _editor.SetCursorPosition(_positionBefore + _text.Length);
            _editor.DeleteText(_text.Length);
        }
    }

    public class DeleteTextCommand : IEditorCommand
    {
        private readonly TextEditor _editor;
        private readonly int _length;
        private int _positionBefore;
        private string _deletedText = "";

        public DeleteTextCommand(TextEditor editor, int length)
        {
            _editor = editor;
            _length = length;
        }

        public void Execute()
        {
            _positionBefore = _editor.GetCursorPosition();
            _deletedText = _editor.DeleteText(_length);
        }

        public void Undo()
        {
            // desfazer delete = inserir de volta no lugar certo
            _editor.SetCursorPosition(_positionBefore - _deletedText.Length);
            _editor.InsertText(_deletedText);
        }
    }

    public class BoldCommand : IEditorCommand
    {
        private readonly TextEditor _editor;
        private readonly int _start;
        private readonly int _length;

        public BoldCommand(TextEditor editor, int start, int length)
        {
            _editor = editor;
            _start = start;
            _length = length;
        }

        public void Execute() => _editor.SetBold(_start, _length);

        public void Undo() => _editor.RemoveBold(_start, _length);
    }

    // ============================
    // 4) INVOKER (histórico Undo/Redo)
    // ============================
    public class CommandHistory
    {
        private readonly Stack<IEditorCommand> _undoStack = new();
        private readonly Stack<IEditorCommand> _redoStack = new();

        public void ExecuteCommand(IEditorCommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear(); // ao executar algo novo, redo anterior não faz sentido
        }

        public void Undo()
        {
            if (_undoStack.Count == 0)
            {
                Console.WriteLine("⚠️ Nada para desfazer.");
                return;
            }

            var cmd = _undoStack.Pop();
            cmd.Undo();
            _redoStack.Push(cmd);
        }

        public void Redo()
        {
            if (_redoStack.Count == 0)
            {
                Console.WriteLine("⚠️ Nada para refazer.");
                return;
            }

            var cmd = _redoStack.Pop();
            cmd.Execute();
            _undoStack.Push(cmd);
        }
    }

    // ============================
    // 5) CLIENT (aplicação)
    // ============================
    public class EditorApplication
    {
        private readonly TextEditor _editor = new();
        private readonly CommandHistory _history = new();

        public void TypeText(string text)
            => _history.ExecuteCommand(new InsertTextCommand(_editor, text));

        public void DeleteCharacters(int count)
            => _history.ExecuteCommand(new DeleteTextCommand(_editor, count));

        public void MakeBold(int start, int length)
            => _history.ExecuteCommand(new BoldCommand(_editor, start, length));

        public void Undo() => _history.Undo();
        public void Redo() => _history.Redo();

        public void ShowContent()
        {
            Console.WriteLine($"\n=== Conteúdo do Editor ===");
            Console.WriteLine($"'{_editor.GetContent()}'");
            Console.WriteLine($"Cursor na posição: {_editor.GetCursorPosition()}\n");
        }
    }

    // ============================
    // 6) DEMO
    // ============================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Editor de Texto (Command + Undo/Redo) ===\n");

            var app = new EditorApplication();

            Console.WriteLine("=== Operações ===");
            app.TypeText("Hello");
            app.TypeText(" World");
            app.ShowContent();

            app.DeleteCharacters(6); // remove " World"
            app.ShowContent();

            app.MakeBold(0, 5); // formatação (simulada)

            Console.WriteLine("\n=== Undo (3x) ===");
            app.Undo(); // desfaz bold
            app.Undo(); // desfaz delete (volta " World")
            app.Undo(); // desfaz insert " World"
            app.ShowContent();

            Console.WriteLine("\n=== Redo (2x) ===");
            app.Redo(); // refaz insert " World"
            app.Redo(); // refaz delete
            app.ShowContent();

            Console.WriteLine("✅ Agora dá para desfazer/refazer qualquer operação via histórico.");
        }
    }
}