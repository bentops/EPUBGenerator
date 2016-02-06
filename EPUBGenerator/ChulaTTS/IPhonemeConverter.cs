namespace TTS
{
    public interface IPhonemeConverter
    {
        string Convert(string input);
        string[] C2Pronunciation(string input);
    }
}
