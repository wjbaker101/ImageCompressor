﻿using System;

namespace ImageCompressor.Main
{
    public static class Utils
    {
        public static bool IsInt(object possibleInt)
        {
            try
            {
                Convert.ToInt32(possibleInt);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
