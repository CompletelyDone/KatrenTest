using System;
using System.IO;

namespace TestTask
{
    public class ReadOnlyStream : IReadOnlyStream
    {
        private Stream _localStream;
        /// <summary>
        /// Конструктор класса. 
        /// Т.к. происходит прямая работа с файлом, необходимо 
        /// обеспечить ГАРАНТИРОВАННОЕ закрытие файла после окончания работы с таковым!
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        public ReadOnlyStream(string fileFullPath)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read);
                _localStream = new MemoryStream();
                fileStream.CopyTo(_localStream);
                _localStream.Position = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new NotImplementedException();
            }
            finally
            {
                fileStream?.Dispose();
            }
        }

        /// <summary>
        /// Флаг окончания файла.
        /// </summary>
        public bool IsEof{ get; private set; }

        public void Dispose()
        {
            _localStream?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Ф-ция чтения следующего символа из потока.
        /// Если произведена попытка прочитать символ после достижения конца файла, метод 
        /// должен бросать соответствующее исключение
        /// </summary>
        /// <returns>Считанный символ.</returns>
        public char ReadNextChar()
        {
            if (_localStream == null) throw new InvalidOperationException();

            int byteRead = _localStream.ReadByte();

            if (byteRead == -1)
            {
                IsEof = true;
                //throw new EndOfStreamException("End of stream is reached.");
            }

            return (char)byteRead;

            // TODO : Необходимо считать очередной символ из _localStream
        }

        /// <summary>
        /// Сбрасывает текущую позицию потока на начало.
        /// </summary>
        public void ResetPositionToStart()
        {
            if (_localStream == null)
            {
                IsEof = true;
                return;
            }

            _localStream.Position = 0;
            IsEof = false;
        }
    }
}
