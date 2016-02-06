namespace TTS.Preprocessors
{
    class Dummy : IPreprocessor
    {
        public string Process(string input)
        {
            return input;
        }
    }
}
