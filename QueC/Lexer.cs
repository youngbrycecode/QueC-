using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace QueC
{
   internal class Lexer
   {
      private const int MaxBufferSize = 1000;

      private StreamReader mInputStream;
      public char[] mCurrentBuffer = new char[MaxBufferSize];
      private int mCurrentBufferPosition = MaxBufferSize + 1;

      private int mCurrentLine = 0;
      private int mCurrentLineIndex = 0;
      private char mCurrentCharacter = '\0';
      private Token mCurrentToken;

      private ReserveTable mKeywords = new ReserveTable();
      private ReserveTable mTypes = new ReserveTable();

      public int EntryPtCode { get; init; }
      public int FnCode { get; init; }

      public int Int8Code { get; init; }
      public int Int16Code { get; init; }
      public int Int32Code { get; init; }
      public int Int64Code { get; init; }
      public int Float32Code { get; init; }
      public int Float64Code { get; init; }
      public int BooleanCode { get; init; }
      public int CharCode { get; init; }
      public int AsciiCharCode { get; init; }

      public Lexer(StreamReader inputStream) 
      {
         // Add keywords.
         EntryPtCode = mKeywords.AddReserveWord("entrypt");
         EntryPtCode = mKeywords.AddReserveWord("func");

         // Add the primitive types.
         Int8Code = mTypes.AddReserveWord("int8");
         Int16Code = mTypes.AddReserveWord("int16");
         Int32Code = mTypes.AddReserveWord("int32");
         Int64Code = mTypes.AddReserveWord("int64");
         Float32Code = mTypes.AddReserveWord("float32");
         Float64Code = mTypes.AddReserveWord("float64");
         BooleanCode = mTypes.AddReserveWord("boolean");
         CharCode = mTypes.AddReserveWord("char");
         AsciiCharCode = mTypes.AddReserveWord("achar");

         mInputStream = inputStream;

         Trace.Assert(inputStream != null);
      }

      public bool NextCharacter()
      {
         if (mCurrentBufferPosition >= MaxBufferSize)
         {
            if (mInputStream.EndOfStream)
            {
               mCurrentCharacter = '\0';
               return false;
            }

            // Load the next buffer if we have to.
            mCurrentBufferPosition = 0;
            mInputStream.Read(mCurrentBuffer, mCurrentBufferPosition, MaxBufferSize);
         }

         mCurrentCharacter = mCurrentBuffer[mCurrentBufferPosition++];
         mCurrentLineIndex++;
         return true;

      }

      /// <summary>
      /// Removes the whitespace from the file.
      /// </summary>
      private bool TrimWhiteSpace()
      {
         while (char.IsWhiteSpace(mCurrentCharacter))
         {
            if (mCurrentCharacter== '\n')
            {
               mCurrentLine++;
               mCurrentLineIndex = 0;
            }

            if (!NextCharacter())
            {
               return false;
            }

            // Deal with comment. TODO
         }

         return true;
      }

      /// <summary>
      /// Reads from the file either an identifier or keyword based on the reserve table.
      /// </summary>
      private void ReadIdentifierOrKeyword()
      {
         mCurrentToken.Lexeme = "" + mCurrentCharacter;

         while (mCurrentCharacter == '_' ||
                char.IsLetter(mCurrentCharacter) ||
                char.IsDigit(mCurrentCharacter))
         {
            mCurrentToken.Lexeme += mCurrentCharacter;

            if (!NextCharacter())
            {
               break;
            }
         }

         int reserveTableCode = mKeywords.GetReserveId(mCurrentToken.Lexeme);

         if (reserveTableCode > 0)
         {
            mCurrentToken.Type = Token.eTokenType.Keyword;
            mCurrentToken.TokenId = reserveTableCode;
         }
         else
         {
            int typeTableCode = mTypes.GetReserveId(mCurrentToken.Lexeme);

            if (typeTableCode > 0)
            {
               mCurrentToken.Type = Token.eTokenType.Type;
               mCurrentToken.TokenId = typeTableCode;
            }
            else
            {
               mCurrentToken.Type = Token.eTokenType.Identifier;
            }
         }
      }


      /// <summary>
      /// Reads a number from the stream.
      /// </summary>
      private bool ReadNumber()
      {
         mCurrentToken.Lexeme = "" + mCurrentToken;
         if (mCurrentCharacter == '-')
         {
            if (!NextCharacter())
            {
               // Failed to read a number in.
               return false;
            }
         }

         bool floatingPoint = false;

         // Load a hex number if that's what this is
         if (mCurrentCharacter == '0')
         {
            if (!NextCharacter()) return false;
            if (mCurrentCharacter == 'b')
            {
               if (!NextCharacter()) return false;

               // Load binary.
               while (mCurrentCharacter == '0' || mCurrentCharacter == '1')
               {
                  mCurrentToken.Lexeme += mCurrentCharacter;
                  if (!NextCharacter()) break;
               }

               mCurrentToken.Type = Token.eTokenType.BinaryInteger;
               return true;
            }
            else if (mCurrentCharacter == 'x')
            {
               if (!NextCharacter()) return false;

               // Load hex.
               while ((mCurrentCharacter >= '0' && mCurrentCharacter <= '9') ||
                  (mCurrentCharacter >= 'A' && mCurrentCharacter <= 'F') ||
                  mCurrentCharacter >= 'a' && mCurrentCharacter <= 'f')
               {
                  mCurrentToken.Lexeme += mCurrentCharacter;
                  if (!NextCharacter()) break;
               }

               mCurrentToken.Type = Token.eTokenType.HexInteger;
               return true;
            }
            else if (buffer[bufferIndex + 1] == 'o')
            {
               // Load octal.
               bufferIndex += 2;
               while (!isEndOfStream() && buffer[bufferIndex] >= '0' && buffer[bufferIndex] <= '7')
               {
                  bufferIndex++;
               }
               currentToken.lexeme = std::string(lexemeStart, buffer + bufferIndex);
               currentToken.code = OCTAL_INTER_CODE;
               return;
            }
         }

         // Assume it's an integer until we see one of a few things:
         // A decimal point turns it into a float
         // e/E for expoential notation turns it into a float
         while (!isEndOfStream() && isNum(buffer[bufferIndex]))
         {
            bufferIndex++;
         }

         // When its not a number anymore, we must see what it is!
         if (!isEndOfStream())
         {
            if (buffer[bufferIndex] == 'e' || buffer[bufferIndex] == 'E')
            {
               bufferIndex++;

               // Optional negative on exponent
               if (!isEndOfStream() && buffer[bufferIndex] == '-')
               {
                  bufferIndex++;
               }

               // Load the rest of the number.
               if (!isEndOfStream() && isNum(buffer[bufferIndex]))
               {
                  while (!isEndOfStream() && isNum(buffer[bufferIndex]))
                  {
                     bufferIndex++;
                  }
               }

               floatingPoint = true;
            }
            else if (buffer[bufferIndex] == '.')
            {
               bufferIndex++;
               if (!isEndOfStream())
               {
                  // Load the rest of the number.
                  while (!isEndOfStream() && isNum(buffer[bufferIndex]))
                  {
                     bufferIndex++;
                  }
               }

               floatingPoint = true;
            }
         }

         currentToken.lexeme = std::string(lexemeStart, buffer + bufferIndex);
         if (floatingPoint)
         {
            currentToken.code = FLOAT_CODE;
         }
         else
         {
            currentToken.code = INTEGER_CODE;
         }
      }


      public bool GetNextToken(Token token)
      {
         mCurrentToken = token;
         TrimWhiteSpace();

         // Read the keyword, identifier, constant, or valid symbol
         if (char.IsLetter(mCurrentCharacter))
         {
            ReadIdentifierOrKeyword();
         }
         else if (char.IsNumber(mCurrentCharacter) || mCurrentCharacter == '-')
         {
            ReadNumber();
         }
         else if (mCurrentCharacter == '"')
         {
            ReadString();
         }
         else if (ReadCharKeyword())
         {
         }
         else
         {
            // An error occurs in this case. 
            // Move till we get to the next word, and log the error.
            // TODO!
         }

         return true;
      }

      public class Token
      {
         public Token()
         {
         }

         public enum eTokenType
         {
            None,
            Keyword,
            BinaryInteger,
            OctalInteger,
            Integer,
            HexInteger,
            FloatingPoint,
            String,
            SyntaxToken,
            Type,
            Identifier
         }

         public string Lexeme { get; set; }
         public int TokenId { get; set; }
         public int LineNum { get; set; }
         public int CharIndex { get; set; }
         public eTokenType Type { get; set; }
      }
   }
}
