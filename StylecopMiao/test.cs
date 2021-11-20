using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StylecopMiao
{
    class Test
    {
        static void Main(string[] args)
        {
            var cat = new ACat();
            Console.WriteLine(cat.name.ToString()); //此处由两条MethodInvocationExpression Token一个为：cat.name.ToString 另一个为 Console.WriteLine
        }
    }
    class ACat
    {
       public string name = "miaomiao";
       public void Say(string word)
       {
            string ans1;
            ans1 = word.ToString(); //报警：未配置参数 
            Console.WriteLine(ans1);

        }
    }
}
