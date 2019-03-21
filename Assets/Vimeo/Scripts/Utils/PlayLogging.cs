
using UnityEngine;
using System;

namespace Vimeo
{
    [Serializable]
    public class PlayLogging
    {
      public string session_id;
      public float furthest_watched_time_code;
      public float exit_watched_time_code;
      public string vuid;
      public string locale;

      public PlayLogging(string _session_id,
          float _furthest_watched_time_code,
          float _exit_watched_time_code,
          string _vuid,
          string _locale="en_US") {
        session_id = _session_id;
        furthest_watched_time_code = _furthest_watched_time_code;
        exit_watched_time_code = _exit_watched_time_code;
        vuid = _vuid;
        locale = _locale;
      }
    }
}