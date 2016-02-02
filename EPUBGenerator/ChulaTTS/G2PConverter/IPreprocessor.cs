namespace ChulaTTS.G2PConverter
{
    public interface IPreprocessor
    {
        string Process(string input, int type);
    }
}
