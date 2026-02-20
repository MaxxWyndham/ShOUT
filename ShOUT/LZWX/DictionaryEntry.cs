namespace ShOUT.LZWX
{
    public class DictionaryEntry
    {
        public int Offset { get; set; }

        public int Length { get; set; }

        public DictionaryEntry(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }
    }
}
