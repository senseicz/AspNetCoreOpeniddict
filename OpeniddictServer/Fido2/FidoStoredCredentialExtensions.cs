namespace OpeniddictServer.Fido2
{
    public static class FidoStoredCredentialExtensions
    {
        public static List<string> ToListOfBase64String(this List<byte[]> data)
        {
            if (data != null && data.Count > 0)
            {
                var retList = new List<string>();
                foreach (var item in data)
                {
                    if (item != null)
                    {
                        retList.Add(Convert.ToBase64String(item, Base64FormattingOptions.None));
                    }
                }

                return retList;
            }

            return null;
        }

        public static List<byte[]> FromListOfBase64String(this List<string> data)
        {
            if (data != null && data.Count > 0)
            {
                var retList = new List<byte[]>();
                foreach (var item in data)
                {
                    retList.Add(Convert.FromBase64String(item));
                }

                return retList;
            }

            return null;
        }
    }
}
