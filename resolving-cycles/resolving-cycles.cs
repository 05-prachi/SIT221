namespace Graph
{
    public class CourseGraph
    {
        // adjacency list
        private readonly Dictionary<string, List<string>> adj = new Dictionary<string, List<string>>();

        public void AddCourse(string c)
        {
            if (!adj.ContainsKey(c)) adj[c] = new List<string>();
        }

        // 'course' requires 'prerequisite'
        public void AddPrerequisite(string course, string prerequisite)
        {
            AddCourse(course); AddCourse(prerequisite);
            adj[prerequisite].Add(course);
        }

        public void RemovePrerequisite(string course, string prerequisite)
        {
            if (adj.ContainsKey(prerequisite)) adj[prerequisite].Remove(course);
        }

        // cycle detection (DFS with 3 colours)
        private enum State { White, Gray, Black }   // unvisited / on current path / finished

        // returns the first cycle found as a list of courses (first == last), or null.
        public List<string> FindCycle()
        {
            var state = adj.Keys.ToDictionary(k => k, k => State.White);
            var path = new List<string>();

            foreach (var start in adj.Keys)
            {
                if (state[start] == State.White)
                {
                    var cycle = Dfs(start, state, path);
                    if (cycle != null) return cycle;
                }
            }
            return null!;
        }

        private List<string> Dfs(string u, Dictionary<string, State> state, List<string> path)
        {
            state[u] = State.Gray;
            path.Add(u);

            foreach (var v in adj[u])
            {
                if (state[v] == State.Gray)
                {
                    // back edge -> cycle: slice the path from v to end, close the loop
                    int idx = path.IndexOf(v);
                    var cycle = path.GetRange(idx, path.Count - idx);
                    cycle.Add(v);
                    return cycle;
                }
                if (state[v] == State.White)
                {
                    var cycle = Dfs(v, state, path);
                    if (cycle != null) return cycle;
                }
            }

            path.RemoveAt(path.Count - 1);
            state[u] = State.Black;
            return null!;
        }

        // topological sort (DFS, reverse post-order)
        public List<string> TopologicalSort()
        {
            if (FindCycle() != null)
                throw new InvalidOperationException("Graph has a cycle; topological order does not exist.");

            var visited = new HashSet<string>();
            var stack = new Stack<string>();

            foreach (var c in adj.Keys)
                if (!visited.Contains(c)) TopoDfs(c, visited, stack);

            return stack.ToList();   // popping order = reverse finishing order
        }

        private void TopoDfs(string u, HashSet<string> visited, Stack<string> stack)
        {
            visited.Add(u);
            foreach (var v in adj[u])
                if (!visited.Contains(v)) TopoDfs(v, visited, stack);
            stack.Push(u);
        }

        public void Print()
        {
            foreach (var kv in adj)
                Console.WriteLine($"  {kv.Key} -> [{string.Join(", ", kv.Value)}]");
        }
    }

    class Program
    {
        static void Main()
        {
            var g = new CourseGraph();
            g.AddPrerequisite("Data Structures", "Programming Fundamentals");
            g.AddPrerequisite("Algorithms", "Data Structures");
            g.AddPrerequisite("Artificial Intelligence", "Algorithms");
            g.AddPrerequisite("Programming Fundamentals", "Artificial Intelligence"); // creates the cycle

            Console.WriteLine("Adjacency list (prerequisite -> courses that need it):");
            g.Print();

            var cycle = g.FindCycle();
            if (cycle != null)
            {
                Console.WriteLine("\nCycle detected: " + string.Join(" -> ", cycle));

                // Resolve: Programming Fundamentals should not require AI
                g.RemovePrerequisite("Programming Fundamentals", "Artificial Intelligence");
                Console.WriteLine("\nRemoved prerequisite: Programming Fundamentals requires Artificial Intelligence");
            }

            Console.WriteLine("\nCycle after fix? " + (g.FindCycle() == null ? "No" : "Yes"));
            Console.WriteLine("Valid course completion order:");
            int i = 1;
            foreach (var c in g.TopologicalSort()) Console.WriteLine($"  {i++}. {c}");
        }
    }
}