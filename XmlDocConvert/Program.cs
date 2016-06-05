using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace XmlDocConvert
{

    sealed class ProjectMember
    {
        public string Role { get; private set; }
        public string Name { get; private set; }
        public ProjectMember(string role, string name)
        {
            this.Role = role ?? string.Empty;
            this.Name = name ?? string.Empty;
        }
    }

    sealed class Project
    {
        public string Name { get; private set; }
        public ReadOnlyCollection<ProjectMember> Members { get; private set; }
        public Project(string name, IList<ProjectMember> members)
        {
            if ((object)members != null && members.Contains(null))
                throw new ArgumentException("Member list must contains all not null values.", "members");
            this.Name = name ?? string.Empty;
            this.Members = new ReadOnlyCollection<ProjectMember>(members ?? new ProjectMember[0]);
        }
    }

    sealed class MemberRole
    {
        public string Name { get; private set; }
        public string Project { get; private set; }
        public MemberRole(string name, string project)
        {
            this.Name = name ?? string.Empty;
            this.Project = project ?? string.Empty;
        }
    }

    sealed class Member
    {
        public string Name { get; private set; }
        public ReadOnlyCollection<MemberRole> Roles { get; private set; }
        public Member(string name, IList<MemberRole> roles)
        {
            if ((object)roles != null && roles.Contains(null))
                throw new ArgumentException("Role list must contains all not null values.", "roles");
            this.Name = name ?? string.Empty;
            this.Roles = new ReadOnlyCollection<MemberRole>(roles ?? new MemberRole[0]);
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

    sealed class XmlDocConverter
    {
        private readonly Func<string> input;
        private readonly Action<string> output;

        public XmlDocConverter(Func<string> input, Action<string> output)
        {
            this.input = input ?? Console.ReadLine;
            this.output = output ?? Console.WriteLine;
        }

        public IList<Project> ReadProjects()
        {
            if (this.input() != InputConsts.ProjectsHeader)
                throw new ArgumentException("Projects header was expected.");

            List<Project> inputProjects = new List<Project>();
            string projectHeader;
            while ((projectHeader = this.input()) != InputConsts.ProjectsTail)
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

                List<ProjectMember> projectMembers = new List<ProjectMember>();
                string memberLine;
                while ((memberLine = this.input()) != InputConsts.ProjectTail)
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

                    projectMembers.Add(new ProjectMember(memberRole, memberName));
                } // while (member list)

                inputProjects.Add(new Project(projectName, projectMembers));
            } // while (project list)

            return inputProjects;
        }

        public void WriteProjects(IList<Project> projects)
        {
            this.output(InputConsts.ProjectsHeader);

            for (int i = 0; i < projects.Count; i++)
            {
                this.output(InputConsts.ProjectHeaderStart + projects[i].Name + InputConsts.ProjectHeaderEnd);
                for (int j = 0; j < projects[i].Members.Count; j++)
                {
                    this.output(
                        InputConsts.ProjectMemberStart +
                        projects[i].Members[j].Role +
                        InputConsts.ProjectMemberNameStart +
                        projects[i].Members[j].Name +
                        InputConsts.ProjectMemberEnd
                    );
                }
                this.output(InputConsts.ProjectTail);
            }

            this.output(InputConsts.ProjectsTail);
        }
    }

    static class Program
    {
        static void Main(string[] args)
        {
            XmlDocConverter converter = new XmlDocConverter(null, null);
            IList<Project> inputProjects = converter.ReadProjects();

            //converter.WriteProjects(inputProjects);
            //Console.ReadLine();
        }
    }

}
