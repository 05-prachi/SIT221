// Question 3: Optimising Student Record Search Using Binary Search Trees and Hashing
namespace BinaryTree
{
    public class Student
    {
        public int RegistrationNumber { get; }
        public string Name { get; set; }
        public string Programme { get; set; }
        public double CGPA { get; set; }

        public Student(int reg, string name, string programme, double cgpa)
        {
            RegistrationNumber = reg;
            Name = name;
            Programme = programme;
            CGPA = cgpa;
        }

        public override string ToString() =>
            $"Reg={RegistrationNumber}\nName={Name}\nProgramme={Programme}\nCGPA={CGPA:F2}";

        class BSTNode
        {
            public Student Data;
            public BSTNode? Left, Right;
            public BSTNode(Student s) { Data = s; }
        }

        public class StudentBST
        {
            private BSTNode? root;
            public bool Insert(Student s)
            {
                if (root == null) { root = new BSTNode(s); return true; }
                BSTNode cur = root;
                while (true)
                {
                    if (s.RegistrationNumber == cur.Data.RegistrationNumber) return false; // keys are unique
                    if (s.RegistrationNumber < cur.Data.RegistrationNumber)
                    {
                        if (cur.Left == null) { cur.Left = new BSTNode(s); return true; }
                        cur = cur.Left;
                    }
                    else
                    {
                        if (cur.Right == null) { cur.Right = new BSTNode(s); return true; }
                        cur = cur.Right;
                    }
                }
            }

            public Student Search(int reg)
            {
                BSTNode cur = root!;
                while (cur != null)
                {
                    if (reg == cur.Data.RegistrationNumber) return cur.Data;
                    cur = reg < cur.Data.RegistrationNumber ? cur.Left! : cur.Right!;
                }
                return null!;
            }

            public bool Update(int reg, string name, string programme, double cgpa)
            {
                Student s = Search(reg);
                if (s == null) return false;
                s.Name = name;
                s.Programme = programme;
                s.CGPA = cgpa;
                return true;
            }

            public bool Delete(int reg)
            {
                bool deleted = false;
                root = Delete(root!, reg, ref deleted);
                return deleted;
            }

            private BSTNode Delete(BSTNode node, int reg, ref bool deleted)
            {
                if (node == null) return null!;

                if (reg < node.Data.RegistrationNumber)
                    node.Left = Delete(node.Left!, reg, ref deleted);
                else if (reg > node.Data.RegistrationNumber)
                    node.Right = Delete(node.Right!, reg, ref deleted);
                else
                {
                    deleted = true;

                    // Case 1: no children
                    if (node.Left == null && node.Right == null) return null!;

                    // Case 2: one child
                    if (node.Left == null) return node.Right!;
                    if (node.Right == null) return node.Left;

                    // Case 3: two children -> replace with inorder successor (min of right subtree)
                    BSTNode succ = node.Right;
                    while (succ.Left != null) succ = succ.Left;
                    node.Data = succ.Data;
                    bool dummy = false;
                    node.Right = Delete(node.Right, succ.Data.RegistrationNumber, ref dummy);
                }

                return node;
            }

            // ---- Inorder traversal: O(n), sorted by registration number ----
            public void DisplayInorder() => Inorder(root!);
            private void Inorder(BSTNode n)
            {
                if (n == null) return;
                Inorder(n.Left!);
                Console.WriteLine("  " + n.Data);
                Inorder(n.Right!);
            }

            public int Height() => Height(root!);
            private int Height(BSTNode n) => n == null ? 0 : 1 + Math.Max(Height(n.Left!), Height(n.Right!));
        }

        public class StudentHashTable
        {
            private readonly LinkedList<Student>[] buckets;
            public StudentHashTable(int size = 211) { buckets = new LinkedList<Student>[size]; }

            private int Hash(int reg) => (reg & 0x7FFFFFFF) % buckets.Length;

            public void Insert(Student s)
            {
                int h = Hash(s.RegistrationNumber);
                buckets[h] ??= new LinkedList<Student>();
                foreach (var x in buckets[h]) if (x.RegistrationNumber == s.RegistrationNumber) return;
                buckets[h].AddLast(s);
            }

            public Student Search(int reg)
            {
                var b = buckets[Hash(reg)];
                if (b == null) return null!;
                foreach (var x in b) if (x.RegistrationNumber == reg) return x;
                return null!;
            }

            public bool Delete(int reg)
            {
                var b = buckets[Hash(reg)];
                if (b == null) return false;
                for (var n = b.First; n != null; n = n.Next)
                    if (n.Value.RegistrationNumber == reg) { b.Remove(n); return true; }
                return false;
            }
        }

        class Program
        {
            static void Main()
            {
                var bst = new StudentBST();
                var ht = new StudentHashTable();
                var students = new[]
                {
                new Student(50, "Asha",   "CS", 3.6),
                new Student(30, "Ravi",   "IT", 3.1),
                new Student(70, "Meera",  "SE", 3.9),
                new Student(20, "Kabir",  "CS", 2.8),
                new Student(40, "Sana",   "AI", 3.4),
                new Student(60, "Dev",    "IT", 3.0),
                new Student(80, "Neha",   "CS", 3.7),
            };
                foreach (var s in students) { bst.Insert(s); ht.Insert(s); }

                Console.WriteLine("Inorder (sorted by registration number):");
                bst.DisplayInorder();

                Console.WriteLine("\nSearch 40 : " + bst.Search(40));
                Console.WriteLine("Search 99 : " + (bst.Search(99)?.ToString() ?? "not found"));

                bst.Update(40, "Sana K.", "AI", 3.8);
                Console.WriteLine("After update: " + bst.Search(40));

                Console.WriteLine("\nDelete 20 (leaf):"); bst.Delete(20); bst.DisplayInorder();
                Console.WriteLine("\nDelete 30 (one child):"); bst.Delete(30); bst.DisplayInorder();
                Console.WriteLine("\nDelete 50 (two children):"); bst.Delete(50); bst.DisplayInorder();

                Console.WriteLine("\nHash table search 70: " + ht.Search(70));

                var skew = new StudentBST();
                for (int i = 1; i <= 1000; i++) skew.Insert(new Student(i, "S" + i, "CS", 3.0));
                Console.WriteLine($"\nAscending inserts of 1000 records -> tree height = {skew.Height()} (balanced would be ~10)");
            }
        }
    }
}