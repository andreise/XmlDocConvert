using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace XmlDocConvert
{

    sealed class InputProjectMember
    {
        public string Role { get; private set; }
        public string Name { get; private set; }
        public InputProjectMember(string role, string name)
        {
            this.Role = role ?? string.Empty;
            this.Name = name ?? string.Empty;
        }
    }

    sealed class InputProject
    {
        public string Name { get; private set; }
        public ReadOnlyCollection<InputProjectMember> Members { get; private set; }
        public InputProject(string name, IList<InputProjectMember> members)
        {
            if ((object)members != null && members.Contains(null))
                throw new ArgumentException("Member list must contains all not null values.", "members");
            this.Name = name ?? string.Empty;
            this.Members = new ReadOnlyCollection<InputProjectMember>(members ?? new InputProjectMember[0]);
        }
    }

    sealed class OutputMemberRole
    {
        public string Name { get; private set; }
        public string Project { get; private set; }
        public OutputMemberRole(string name, string project)
        {
            this.Name = name ?? string.Empty;
            this.Project = project ?? string.Empty;
        }
    }

    sealed class OutputMember
    {
        public string Name { get; private set; }
        public ReadOnlyCollection<OutputMemberRole> Roles { get; private set; }
        public OutputMember(string name, IList<OutputMemberRole> roles)
        {
            if ((object)roles != null && roles.Contains(null))
                throw new ArgumentException("Role list must contains all not null values.", "roles");
            this.Name = name ?? string.Empty;
            this.Roles = new ReadOnlyCollection<OutputMemberRole>(roles ?? new OutputMemberRole[0]);
        }
    }

    static class InputConsts
    {
        public const string ProjectsHeader = "<projects>";
        public const string ProjectsTail = "</projects>";

        public const string ProjectHeaderStart = "    <project name=\"";
        public const string ProjectHeaderEnd = "\">";
        public static readonly int ProjectHeaderMinLength = ProjectHeaderStart.Length + ProjectHeaderEnd.Length;
        public const string ProjectTail = "    </project>";

        public const string ProjectMemberStart = "        <member role=\"";
        public const string ProjectMemberEnd = "\"/>";
        public const string ProjectMemberNameStart = "\" name=\"";
        public static readonly int ProjectMemberMinLength = ProjectMemberStart.Length + ProjectMemberNameStart.Length + ProjectMemberEnd.Length;

        static InputConsts() { }
    }

    static class Program
    {

        static IList<InputProject> ReadInputProjects()
        {
            if (Console.ReadLine() != InputConsts.ProjectsHeader)
                throw new ArgumentException("Projects header was expected.");

            List<InputProject> inputProjects = new List<InputProject>();
            string projectHeader;
            while ((projectHeader = Console.ReadLine()) != InputConsts.ProjectsTail)
            {
                if (!(
                    projectHeader.Length >= InputConsts.ProjectHeaderMinLength &&
                    projectHeader.StartsWith(InputConsts.ProjectHeaderStart, StringComparison.Ordinal) &&
                    projectHeader.EndsWith(InputConsts.ProjectHeaderEnd, StringComparison.Ordinal)
                ))
                    throw new ArgumentException("Project header is incorrect.");

                string projectName = projectHeader.Substring(
                    InputConsts.ProjectHeaderStart.Length,
                    projectHeader.Length - (InputConsts.ProjectHeaderStart.Length + InputConsts.ProjectHeaderEnd.Length)
                );

                List<InputProjectMember> projectMembers = new List<InputProjectMember>();
                string memberLine;
                while ((memberLine = Console.ReadLine()) != InputConsts.ProjectTail)
                {
                    if (!(
                        memberLine.Length >= InputConsts.ProjectMemberMinLength &&
                        memberLine.StartsWith(InputConsts.ProjectMemberStart, StringComparison.Ordinal) &&
                        memberLine.EndsWith(InputConsts.ProjectMemberEnd, StringComparison.Ordinal) &&
                        memberLine.Contains(InputConsts.ProjectMemberNameStart)
                    ))
                        throw new ArgumentException("Member header is incorrect.");

                    int indexOfProjectMemberNameStart = memberLine.IndexOf(InputConsts.ProjectMemberNameStart, StringComparison.Ordinal);

                    string memberRole = memberLine.Substring(
                        InputConsts.ProjectMemberStart.Length,
                        memberLine.Length - (indexOfProjectMemberNameStart + InputConsts.ProjectMemberNameStart.Length) + 1
                    );

                    string memberName = memberLine.Substring(
                        indexOfProjectMemberNameStart + InputConsts.ProjectMemberNameStart.Length,
                        memberLine.Length - (indexOfProjectMemberNameStart + InputConsts.ProjectMemberNameStart.Length + InputConsts.ProjectMemberEnd.Length)
                    );

                    projectMembers.Add(new InputProjectMember(memberRole, memberName));
                } // while (member list)

                inputProjects.Add(new InputProject(projectName, projectMembers));
            } // while (project list)

            return inputProjects;
        }

        static void OutputProjects(IList<InputProject> projects)
        {
            Console.WriteLine(InputConsts.ProjectsHeader);

            for (int i = 0; i < projects.Count; i++)
            {
                Console.WriteLine(InputConsts.ProjectHeaderStart + projects[i].Name + InputConsts.ProjectHeaderEnd);
                for (int j = 0; j < projects[i].Members.Count; j++)
                {
                    Console.WriteLine(
                        InputConsts.ProjectMemberStart +
                        projects[i].Members[j].Role +
                        InputConsts.ProjectMemberNameStart +
                        projects[i].Members[j].Name +
                        InputConsts.ProjectMemberEnd
                    );
                }
                Console.WriteLine(InputConsts.ProjectTail);
            }

            Console.WriteLine(InputConsts.ProjectsTail);
        }

        static void Main(string[] args)
        {
            IList<InputProject> inputProjects = ReadInputProjects();

            //OutputProjects(inputProjects);

            //Console.ReadLine();
        }
    }

}
