// Question 1: Dynamic Task Scheduling Using a Priority Queue
namespace PriorityQueue
{
    public class Task
    {
        public int TaskID { get; }
        public int Priority { get; }
        public int ExecutionTime { get; }      // minutes
        public DateTime ArrivalTime { get; }
        internal long Sequence;                // tie-breaker for identical priority AND arrival time

        public Task(int id, int priority, int execTime, DateTime arrival)
        {
            TaskID = id;
            Priority = priority;
            ExecutionTime = execTime;
            ArrivalTime = arrival;
        }

        public override string ToString() =>
            $"ID={TaskID}\nPriority={Priority}\nExec={ExecutionTime}min\nArrival={ArrivalTime:HH:mm:ss.ff}";
    }

    public class TaskPriorityQueue
    {
        private readonly List<Task> heap = new List<Task>();   // array-based binary max-heap
        private long counter = 0;                              // increasing sequence number

        public int Count => heap.Count;

        private static bool HigherPriority(Task a, Task b)
        {
            if (a.Priority != b.Priority) return a.Priority > b.Priority;
            if (a.ArrivalTime != b.ArrivalTime) return a.ArrivalTime < b.ArrivalTime;
            return a.Sequence < b.Sequence;   // same priority & arrival: first inserted goes first
        }

        public void Insert(Task t)
        {
            t.Sequence = counter++;
            heap.Add(t);
            SiftUp(heap.Count - 1);
        }

        public Task ExtractHighest()
        {
            if (heap.Count == 0) throw new InvalidOperationException("No pending tasks.");
            Task top = heap[0];
            Task last = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);
            if (heap.Count > 0)
            {
                heap[0] = last;
                SiftDown(0);
            }
            return top;
        }

        public Task Peek() =>
            heap.Count == 0 ? throw new InvalidOperationException("Empty.") : heap[0];

        public void DisplayInProcessingOrder()
        {
            if (heap.Count == 0) { Console.WriteLine("  (no pending tasks)"); return; }
            var copy = new TaskPriorityQueue();
            copy.heap.AddRange(heap);
            copy.counter = counter;
            int pos = 1;
            while (copy.Count > 0) Console.WriteLine($"  {pos++}. {copy.ExtractHighest()}");
        }

        private void SiftUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (!HigherPriority(heap[i], heap[parent])) break;
                (heap[i], heap[parent]) = (heap[parent], heap[i]);
                i = parent;
            }
        }

        private void SiftDown(int i)
        {
            int n = heap.Count;
            while (true)
            {
                int l = 2 * i + 1, r = 2 * i + 2, best = i;
                if (l < n && HigherPriority(heap[l], heap[best])) best = l;
                if (r < n && HigherPriority(heap[r], heap[best])) best = r;
                if (best == i) break;
                (heap[i], heap[best]) = (heap[best], heap[i]);
                i = best;
            }
        }
    }

    class Program
    {
        static void Main()
        {
            var pq = new TaskPriorityQueue();
            DateTime t0 = new DateTime(2026, 11, 23, 9, 0, 0);

            pq.Insert(new Task(101, 2, 30, t0));
            pq.Insert(new Task(102, 5, 45, t0.AddMinutes(5)));
            pq.Insert(new Task(103, 5, 20, t0.AddMinutes(2)));   // same priority as 102, earlier arrival
            pq.Insert(new Task(104, 3, 60, t0.AddMinutes(1)));
            pq.Insert(new Task(105, 5, 15, t0.AddMinutes(2)));   // same priority + arrival as 103 -> FIFO

            Console.WriteLine("Pending tasks in processing order:");
            pq.DisplayInProcessingOrder();

            Console.WriteLine("\nProcessing tasks:");
            while (pq.Count > 0) Console.WriteLine("  Processed -> " + pq.ExtractHighest());

            // performance demonstration: 10,000 simultaneous tasks
            var rng = new Random(42);
            var big = new TaskPriorityQueue();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
                big.Insert(new Task(i, rng.Next(1, 11), rng.Next(1, 120), DateTime.Now));
            while (big.Count > 0) big.ExtractHighest();
            sw.Stop();
            Console.WriteLine($"\n10,000 inserts + 10,000 extractions with heap: {sw.ElapsedMilliseconds} ms");
        }
    }
}