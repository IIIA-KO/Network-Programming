namespace CurrenciesServerLibrary
{
    internal class Logger
    {
        private readonly string _filePath;

        public Logger(string filePath)
        {
            this._filePath = filePath;
        }

        public void WriteLog(string log)
        {
            try
            {
                lock (this._filePath)
                {
                    if (!File.Exists(this._filePath))
                    {
                        File.Create(this._filePath).Close();
                    }

                    File.AppendAllText(this._filePath, log);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}