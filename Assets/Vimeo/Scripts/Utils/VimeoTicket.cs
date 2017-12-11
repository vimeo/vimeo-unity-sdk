using UnityEngine;

public class VimeoTicket
{
	public string ticket_id;
	public string uri;
	public string upload_link_secure;
	public string complete_uri;
    public string error;

	public static VimeoTicket CreateFromJSON(string jsonString)
	{
		return JsonUtility.FromJson<VimeoTicket> (jsonString);
	}
}