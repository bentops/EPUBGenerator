namespace ChulaTTS.G2PConverter
{
    public interface IPhonemeConverter
    {
        string Convert(string input, int type);
        string[] C2Pronunciation(string input);
    }
}
