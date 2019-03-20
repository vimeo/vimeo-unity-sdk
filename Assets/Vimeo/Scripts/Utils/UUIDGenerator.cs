
using UnityEngine;

namespace Vimeo
{
    public class UUIDGenerator
    {

        static string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789";

        public string Generate(int length)
        {
          string tempString = "";
          for(int i=0; i<length; i++)
          {
              tempString += UUIDGenerator.glyphs[Random.Range(0, UUIDGenerator.glyphs.Length)];
          }
          return tempString;
        }
    }
}
