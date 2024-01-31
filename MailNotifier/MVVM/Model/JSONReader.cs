using Newtonsoft.Json;
using System.IO;

namespace MailNotifier.MVVM.Model
{
    internal class JSONReader
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;

        public void ReadJson()
        {
            StreamReader streamReader = new("config.json");
            string json = streamReader.ReadToEnd();
            JSONStructure? data = JsonConvert.DeserializeObject<JSONStructure>(json);

            if (data == null)
                return;

            Username = data.Username;
            Password = data.Password;
            Email = data.Email;
        }
    }

    internal class JSONStructure
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
