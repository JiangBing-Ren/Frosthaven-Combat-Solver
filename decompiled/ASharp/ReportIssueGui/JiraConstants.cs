using System;
using System.Collections.Generic;
using System.Text;
using Libs.GameConsole;

namespace ReportIssueGui;

public static class JiraConstants
{
	[ConsoleVariable(Alias = "jira_login", Persistent = true)]
	private static string _jiraAuthOverride = string.Empty;

	public static readonly List<string> Severity = new List<string> { "1 - Critical", "2 - Major", "3 - Moderate", "4 - Minor" };

	public static readonly List<string> Versions = new List<string> { "6 Maimak", "7 Poseidon", "8 Anthest", "9 Mounuch", "10 Skiroph" };

	public static readonly Dictionary<string, string> Users = new Dictionary<string, string>
	{
		{ "Andrey Petrov", "5b44b5591dff302c1c1144ba" },
		{ "Borislav Bogdanov", "557058:1b37c61a-3b28-42a7-8360-2ec2b3e2d80f" },
		{ "Bozhidar Borisov", "5ac695d92778031981e1ef9a" },
		{ "Bozhidar Grozdanov", "5b44b55e2726b92cd5366fa0" },
		{ "Bojidar Petkov", "712020:78a33097-1319-421c-b535-bde4da74f9f5" },
		{ "Catherine Hadjiiska", "5efb9137a115ac0bb52da1d0" },
		{ "Damyan Chokoev", "5e6585df77d46c0cf93ab8c2" },
		{ "Damyan Momchev", "5f5ebb790999c0007735d86e" },
		{ "Danail Trifonov", "5b97696cd1ba852c37e1a73d" },
		{ "Dean Calver", "5adda81954b5e72468e778b0" },
		{ "Denis Rizov", "5aaa3da4655d8c2a1c23727a" },
		{ "Denis Zakharov", "712020:8be7548e-510a-4ddf-aef1-b5613fffc0d6" },
		{ "Dimitar Evtimov", "5b9f5b0c4f58fa18e9f739e1" },
		{ "Elitsa Ilieva", "5c53fe7a9909c944ea2be7db" },
		{ "Elizaveta Seliverstova", "712020:8282927f-e217-41e7-83f7-05bb166ea73b" },
		{ "Filip Slavov", "5aaa3dab655d8c2a1c23727c" },
		{ "Georgi Dimitrov", "5f1c952f9d9a120029a2d580" },
		{ "Georgi Karadenizov", "5d4026e727c72d0daf92b85e" },
		{ "Hristian Hristov", "5b44b56378079611441b3326" },
		{ "Hristo Ivanov", "5c58098d95f91f3a0420449b" },
		{ "Hristo Petkov", "5c59abf345080734f200075c" },
		{ "Ivan Genchev", "5c53ff35825530450d75fdd9" },
		{ "Ivan Klinkov", "712020:dd8f0233-7a97-487b-b89a-0ce620bcd7ac" },
		{ "Julian Gollop", "557058:79811437-9311-46c8-9803-aaacccad3517" },
		{ "Kalin Stanev", "5efb9137d630ac0bb018bb27" },
		{ "Kaloyan Nachev", "5bb1d775c2386b1ffb4a3cfd" },
		{ "Konstantina Ivanova", "5ac230da393fdd4d0ed5c3ce" },
		{ "Kristiyan Damyanov", "5f2da167c1b9f4001cdab10a" },
		{ "Maria Cherneva", "712020:c4b4b240-c40f-4f3d-9850-81c82b60b25a" },
		{ "Martin Marinov", "5aaa4d45f75bb52a8232bc1d" },
		{ "Miron Mironov", "5aaa3daf6996a82a29749608" },
		{ "Miroslav Dimitrov", "5c5b1a1e92db7b0b85249e73" },
		{ "Nedislav Stoychev", "5aaa3db4b238d82a161f17be" },
		{ "Nikola Tasev", "5aaa3db8b238d82a161f17bf" },
		{ "Nikola Petrov", "5f5ebb43b9fcc10077142ddb" },
		{ "Nikolay Berov", "5b6c27fb304ed93c17e427b0" },
		{ "Nina Reznichenko", "6233956673c8ec00699df953" },
		{ "Oni Aslanyan", "5ac230decd9f4b64620618d2" },
		{ "Pavel Sdobnikov", "712020:bb8f270d-ed94-46ce-9334-6466bf70a7d7" },
		{ "Petar Dobrev", "5f4d63ee6db35e0039cfa9cc" },
		{ "Plamen Iliev", "5aaa4d4aef48102a7b9f0752" },
		{ "Prodan Kishishev", "5b8fe4f43e56f62bec08bf00" },
		{ "Radoslav Koynov", "5e786b7f7276650c2d4297ee" },
		{ "Rosen Kazlachev", "5af1a6d9999f392c4882ea17" },
		{ "Rossen Cholakov", "5d19e7a9b591b80c1e31bc8b" },
		{ "Rossen Tzvetanov", "5b9f4cffafe2e024e040ab5b" },
		{ "Sergey Georgiev", "557058:780ebd2e-4368-43d5-9fcf-41bbbc0d6be4" },
		{ "Simeon Dotkov", "5aaa75db18c3bd2a73fe907d" },
		{ "Stanislav Todorov", "5e84405432b0710c1f7e001e" },
		{ "Stoyan Dzhongov", "5c6126689854722ed6776b4e" },
		{ "Stoycho Rusanov", "712020:42013107-96fd-4ea9-b0cf-3506280a2fb5" },
		{ "Svetoslav Petrov", "5aaa4d5536a3a82a4e2ee1a4" },
		{ "Teodor Klissarov", "5ebaf98eed4a650b8e74c366" },
		{ "Tsvetomir Georgiev", "712020:12b53b11-49f2-4ea9-8be1-0c70ba398b3e" },
		{ "Vasil Magaranov", "5b6833de9c10793d9d0999d6" },
		{ "Velin Radenkov", "5dad58f098f7270c2614b80a" },
		{ "Velislava Chavdarova", "5cbda3a89191e80e3d40b16f" },
		{ "Vesselin Jilov", "5aaa4d5d1396802a57aa3299" },
		{ "Vesselina Davidkova", "5bbb076c420fae14bc813b5d" },
		{ "Victoria Hristova", "5ac230e1558cc94758328464" },
		{ "Vit Barta", "5e411f6308e2a40c9383961b" },
		{ "Yanitsa Dimitrova", "5e71dc479858860c3eb102de" },
		{ "[KWS] Agnieszka Jarzynska", "712020:93deb10d-d671-4b73-840c-415f97383b2a" },
		{ "[KWS] Alan Piotrowski", "6273b0f26a38370069dec0f9" },
		{ "[KWS] Paweł Łaskowski", "712020:3daf7cb2-2c8b-4702-aeaa-05be3ff137b6" },
		{ "[KWS] Szymon Myjak", "712020:faa2250d-df2f-463a-b20c-99dc4b793291" },
		{ "[LB] Adam Jarochowski", "712020:e0b0e332-d926-4989-b29d-43cd94d7ecd7" },
		{ "[LB] Arkadiusz Tyminski", "712020:a8a0b93a-6b9e-4535-ba4d-5ba926ebefa6" },
		{ "[LB] Bartosz Kunkowski", "712020:67d3b8ae-144d-4c21-a6da-50da06f0462d" },
		{ "[LB] Kacper Paszkiewicz", "712020:62c981bc-4f25-42d3-a987-706d8258fe65" }
	};

	public static readonly List<string> Components = new List<string>
	{
		"AI", "Animations", "Character Art", "Concept Art", "Environment", "Game Design", "Gameplay", "Level Design", "QA", "SFX",
		"Systems", "Technical Art", "UI", "UI/UX", "VFX"
	};

	public static readonly List<string> Priorities = new List<string> { "MUST FIX", "High", "Medium", "Low" };

	public static readonly List<(string, string)> Categories = new List<(string, string)>
	{
		("Crash - game closes and returns you to desktop", "Crash"),
		("Freeze/Stuck - game remains stuck at a certain section", "Freeze/Stuck"),
		("Broken Mechanics - deviations from the game rules", "Broken Mechanics"),
		("Visuals - visual issues and VFX glitches", "Visuals"),
		("Sound - SFX and music issues", "Sound")
	};

	public static string JiraUserStudio => "customer.support1@snapshotgames.com";

	public static string JiraKeyStudio => "ATATT3xFfGF0BltaPOCjRrdFfprRag9LovQLR4WP2HKb773Wvg6cz9OMB89HbvFxPZpMMjcwEyKz6IVnlJKuEn3vVNWs3oJjoUwnHCL3v4ke1KAdMwAvD3SvYzwAf2m7ZbJfwPDZKhCRxZgUYfDvzNbpUKkCBTESlAQGGt54NxXqbODfr9W-5pg=2580120C";

	public static string JiraUserCustomer => "support@snapshotgames.com";

	public static string JiraKeyCustomer => null;

	public static string JiraKeyCustomerUrl => "https://tickets.snapshotgames.com/EFHI";

	public static string JiraAuthOverride => _jiraAuthOverride;

	public static string ProjectIDStudio => "FH";

	public static string IssueTypeStudio => "Bug";

	public static string ProjectIDCustomer => "FHI";

	public static string IssueTypeCustomer => "Feedback";

	public static string CreateJiraAuth(string user, string key)
	{
		return Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + key));
	}
}
