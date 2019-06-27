using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMiningDeployer.Classes
{
    public class KnowledgeStoreSkillset : Skillset
    {
        public dynamic knowledgeStore { get; set; }
    }
}
