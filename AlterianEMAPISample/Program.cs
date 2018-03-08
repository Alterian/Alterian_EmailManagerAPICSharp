using System;
using System.Linq;
using AlterianEMAPISample.Authenticate;
using AlterianEMAPISample.BizLogic;

namespace AlterianEMAPISample
{
    internal static class Program
    {
        static void Main()
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                var args = input.Split(' ');
                var command = args.First();
                var argument = "";
                var id = "";
                var pass = "";
                if (args.Length == 2)
                {
                    argument = args[1];
                }
                else if (args.Length == 3)
                {
                    id = args[1];
                    pass = args[2];
                }

                EMEmail emEmail;
                EMList emList;
                switch (command)
                {
                    case "login":
                        var result = Auth.Login(id, pass);
                        Console.Clear();
                        Console.WriteLine(result);
                        break;
                    case "listcreate":
                        emList = new EMList();
                        var listId = emList.Create();
                        Console.WriteLine($"Success - Created list id:{listId}");
                        break;
                    case "listupload":
                        emList = new EMList();
                        emList.Upload(int.Parse(argument));
                        Console.WriteLine("Success");
                        break;
                    case "listdownload":
                        emList = new EMList();
                        emList.Download();
                        Console.WriteLine("Success");
                        break;
                    case "listquery":
                        emList = new EMList();
                        emList.Query();
                        Console.WriteLine("Success");
                        break;
                    case "send_anemail":
                        emEmail = new EMEmail();
                        emEmail.SendAnEmail();
                        Console.WriteLine("Success");
                        break;
                    case "send_multipleemails_withdeployment":
                        emEmail = new EMEmail();
                        emEmail.SendMultipleEmailsWithGivenDeployment();
                        Console.WriteLine("Success");
                        break;
                    case "send_multipleemails_withoutdeployment":
                        emEmail = new EMEmail();
                        emEmail.SendMultipleEmailsAfterCreatingDeployment();
                        Console.WriteLine("Success");
                        break;
                    case "createcreative":
                        var emCreative = new EMCreative();
                        emCreative.Create();
                        Console.WriteLine("Success");
                        break;
                    case "exportresponse":
                        var emResponse = new EMResponse();
                        emResponse.Export();
                        Console.WriteLine("Success");
                        break;
                    case "Unsubscribe":
                        var emRecipient = new EMRecipient();
                        emRecipient.Unsubscribe();
                        Console.WriteLine("Success");
                        break;
                    case "exit": 
                        return;
                }
            }
        }
    }
}
