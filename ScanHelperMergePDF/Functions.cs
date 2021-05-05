using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace ScanHelperMergePDF
{
    public static class Functions
    {
        public static bool Like(this string toSearch, string toFind)
        {
            return toFind.Contains("%") ? 
                LikeOperator.LikeString(toSearch, toFind.Replace("%", "*"), CompareMethod.Text) : 
                LikeOperator.LikeString(toSearch, "*" + toFind + "*", CompareMethod.Text);
        }
    }
}
