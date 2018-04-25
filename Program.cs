using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace CppMerger
{
    class GraphNode
    {
        public int Key {get; set;}
        public List<int> Values = new List<int>();

        public GraphNode( int key, int value )
        {
            Key = key;
            Values.Add(value);
        }
        public void Insert(int value)
        {
            Values.Add(value);
        }
    }

    class Program
    {
        static void DoTopologicalSort(GraphNode current, List<GraphNode> nodes, ref HashSet<int> visited, ref List<int> topological)
        {
            Console.WriteLine("visiting node of {0}", current.Key );

            visited.Add( current.Key );

            IEnumerable<GraphNode> neighbors = nodes.Where(Item => current.Values.Any(val => val == Item.Key ) );
            
            foreach (GraphNode child in neighbors)
            {
                if (visited.Contains(child.Key) == false)
                {
                    Console.WriteLine("visiting child node of {0}", child.Key);
                    DoTopologicalSort(child, nodes, ref visited, ref topological);
                }
            }
            
            foreach ( var val in current.Values )
            {
                if (topological.Contains(val) == false)
                    topological.Add(val);
            }

            if(topological.Contains(current.Key) == false)
                topological.Add(current.Key);
        }

        static List<int> TopologicalSort( List<GraphNode> graph)
        {
            List<int> topological = new List<int>();
            HashSet<int> visited = new HashSet<int>();

            foreach (var node in graph)
            {
                if (visited.Contains(node.Key) == true)
                    continue;

                Console.WriteLine("Do toplogical sort for {0}", node.Key);
                DoTopologicalSort(node, graph, ref visited, ref topological);
            }
            topological.Reverse();

            Console.WriteLine( "Topological Order is {0}", topological.ToDebugString() );

            return topological;
        }

        public static string[] ReorgranizeMergeOrder( List<string> files )
        {
            List<string> orderedFiles = new List<string>();

            Dictionary<string, int> dictFilePath = new Dictionary<string, int>();
            for( var i = 0; i < files.Count; i++ )
            {
                dictFilePath[files[i]] = i;
            }
            foreach (var key in dictFilePath.Keys)
            {
                Console.Error.WriteLine("{0} => {1}", key, dictFilePath[key]);
            }

            List<GraphNode> numberOrder = new List<GraphNode>();
            foreach ( string filename in files )
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line;
                    bool isIncluded = false;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("#include") && line.EndsWith(".h\""))
                        {
                            isIncluded = true;
                            //Console.WriteLine(line);
                            string includedFileName = Regex.Match(line, "\".*\"").Groups[0].Value;
                            includedFileName = includedFileName.Substring(1, includedFileName.Length - 2);
                            Console.WriteLine( "{0} is included in {1}", includedFileName, filename );

                            foreach( var fullFilePath in files )
                            {
                                if( fullFilePath.Contains( includedFileName ) == true )
                                {
                                    int before = dictFilePath[fullFilePath];
                                    int after = dictFilePath[filename];

                                    if( numberOrder.Exists(item => item.Key == before) )
                                    {
                                        numberOrder.Find(item => item.Key == before).Insert(after);
                                    }
                                    else
                                    {
                                        numberOrder.Add(new GraphNode(before, after));
                                    }
                                }
                            }
                        }
                    }
                    if(isIncluded == false)
                    {
                        numberOrder.Add(new GraphNode(dictFilePath[filename], dictFilePath[filename]));
                    }
                }
            }

            foreach (var node in numberOrder )
            {
                Console.Write("{0} => ", node.Key);
                foreach (var val in node.Values)
                    Console.Write("{0} ", val );
                Console.WriteLine();
            }

            var numberList = TopologicalSort( numberOrder );
            Console.Write("Merge order = ");
            foreach (var val in numberList)
            {
                Console.Write("{0} ", val);
            }
            Console.WriteLine();

            foreach (var val in numberList)
            {
                orderedFiles.Add(files[val]);
            }

            return orderedFiles.ToArray() ;
        }

        // can't resolve cyclic depencies.
        static void Main(string[] args)
        {
            string searchPath = "D:\\Works\\BitBucket\\CodeRoyale\\CodeRoyale";
            string outputFilePath = "D:\\Works\\BitBucket\\CodeRoyale\\TestSingleFile\\CodeRoyale.cpp";

            // TODO : find better way to describe target path and output path
            // CppMerger -src "..\\CodeRoyale" -output "..\\TestSingleFile\\CodeRoyale.cpp"
            if ( args.Length > 0 )
            {
                int srcIndex = Array.IndexOf(args, "-src");
                if( srcIndex >= 0 && srcIndex + 1 < args.Length )
                {
                    searchPath = args[srcIndex + 1];
                }

                int targetIndex = Array.IndexOf(args, "-output");
                if( targetIndex >= 0 && targetIndex + 1 < args.Length )
                {
                    outputFilePath = args[targetIndex + 1];
                }
            }
            else
            {
                Console.WriteLine("Usage : CppMerger -src sourcePath -output targetFilePath");
            }

            try
            {
                Console.WriteLine(searchPath);

                var files = Directory.GetFiles(searchPath, "*.*")
                    .Where(file => file.ToLower().EndsWith(".h") || file.ToLower().EndsWith(".cpp")).ToList();

                var orderedFiles = ReorgranizeMergeOrder(files);

                var outFile = File.Create(outputFilePath);
                outFile.Close();
                foreach (string filename in orderedFiles)
                {
                    Console.WriteLine(filename);

                    var lines = File.ReadAllLines(filename);
                    var newLines = new List<string>();
                    foreach( var line in lines )
                    {
                        if (line.StartsWith("#include") && line.EndsWith(".h\""))
                        {
                            // do nothing;
                        }
                        else
                        {
                            newLines.Add(line);
                        }
                    }
                    File.AppendAllLines(outputFilePath, newLines);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
