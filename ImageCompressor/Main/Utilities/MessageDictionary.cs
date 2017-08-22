namespace ImageCompressor.Main.Utilities
{
    public static class MessageDictionary
    {
        public static string GetFoundMessage(int quantity)
        {
            return "Found " + quantity + " " + GetPluralImage(quantity);
        }

        public static string GetDisabledMessage(int max, int quantity)
        {
            return GetFoundMessage(max) + " " + quantity + " " + GetPluralIs(quantity) + " disabled.";
        }

        public static string GetCompressedMessage(string current, int max, int quantity)
        {
            return "Compressed " + current + ". Completed " + quantity + " of " + max + " " + GetPluralImage(quantity);
        }

        public static string GetResizedMessage(string current, int max, int quantity)
        {
            return "Resizing " + current + ". Completed " + quantity + " of " + max + " " + GetPluralImage(quantity);
        }

        public static string GetCompletionMessage(int max, int quantity)
        {
            return "Completed " + max + " of " + quantity + " " + GetPluralImage(quantity);
        }

        private static string GetPluralImage(int quantity)
        {
            return (quantity == 1 ? "image" : "images.");
        }

        private static string GetPluralIs(int quantity)
        {
            return (quantity == 1 ? "is" : "are");
        }
    }
}
