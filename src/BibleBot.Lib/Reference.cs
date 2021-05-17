namespace BibleBot.Lib
{
    public class Reference
    {
        public string Book { get; set; }
        
        public int StartingChapter { get; set; }
        public int StartingVerse { get; set; }
        public int EndingChapter { get; set; }
        public int EndingVerse { get; set; }

        public Version Version { get; set; }

        public bool IsOT { get; set; }
        public bool IsNT { get; set; }
        public bool IsDEU { get; set; }

        public override string ToString()
        {
            string result = $"{this.Book} {this.StartingChapter}:{this.StartingVerse}";

            if (this.EndingChapter > 0 && this.EndingChapter != this.StartingChapter)
            {
                result += $"-{this.EndingChapter}:{this.EndingVerse}";
            }
            else if (this.EndingVerse > 0 && this.EndingVerse != this.StartingVerse)
            {
                result += $"-{this.EndingVerse}";
            }
            else if (this.EndingChapter > 0 && this.EndingVerse == 0)
            {
                result += "-";
            }

            return result;
        }
    }
}