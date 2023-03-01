using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueC
{
   internal class Lexer
   {
      public Lexer() 
      { 
      }

      public class Token
      {
         public Token(string lexeme, int line, int charIdx)
         {
            Lexeme = lexeme;
            LineNum = line;
            CharIndex = charIdx;
         }

         public enum eTokenType
         {
            None,
            Keyword,
            Integer8,
            Integer16,
            Integer32,
            Integer64,
            Float32,
            Float64,
            String,
            SyntaxToken
         }

         public string Lexeme { get; init; }
         public int LineNum { get; init; }
         public int CharIndex { get; init; }
      }
   }
}
