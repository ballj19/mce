using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Collections;

namespace ConcealedSquare
{
    public class Robbers
    {
        static int count = 0;

        static void xMain(string[] args)
        {
            var result = Rob(new int[] {226, 174, 214, 16, 218, 48, 153, 131, 128, 17, 157, 142, 88, 43, 37, 157, 43, 221, 191, 68, 206, 23, 225, 82, 54, 118, 111, 46, 80, 49, 245, 63, 25, 194, 72, 80, 143, 55, 209, 18, 55, 122, 65, 66, 177, 101, 63, 201, 172, 130, 103, 225, 142, 46, 86, 185, 62, 138, 212, 192, 125, 77, 223, 188, 99, 228, 90, 25, 193, 211, 84, 239, 119, 234, 85, 83, 123, 120, 131, 203, 219, 10, 82, 35, 120, 180, 249, 106, 37, 169, 225, 54, 103, 55, 166, 124});

            Console.WriteLine(count);
        }

        public static int Rob(int[] nums)
        {

            Func<int[], int, int> rob = RobHouse();


            int[] path1 = nums;
            int result1 = rob(path1, 0);

            int[] path2 = new int[nums.Length - 1];
            Array.Copy(nums, 1, path2, 0, nums.Length - 1);
            int result2 = rob(path2, 1);
            int result = Math.Max(result1, result2);

            return result;
        }

        static Func<int[],int, int> RobHouse()
        {
            Dictionary<int, int> cache = new Dictionary<int, int>();

            Func<int[], int, int> rob = null;

            return rob = (n, i) =>
            {
                count++;
                if (cache.ContainsKey(i))
                {
                    return cache[i];
                }

                if (n.Length == 0)
                {
                    return 0;
                }
                else if (n.Length <= 2)
                {
                    return n[0];
                }
                else
                {
                    int[] path1 = new int[n.Length - 2];
                    Array.Copy(n, 2, path1, 0, n.Length - 2);

                    int[] path2 = new int[n.Length - 3];
                    Array.Copy(n, 3, path2, 0, n.Length - 3);

                    int rob1 = rob(path1, i + 2);
                    int rob2 = rob(path2, i + 3);

                    int result = n[0] + Math.Max(rob1, rob2);
                    cache.Add(i, result);

                    return result;
                }
            };
        }
























        static Func<int,int> Fibonacci()
        {
            Dictionary<int, int> cache = new Dictionary<int, int>();

            Func<int, int> fib = null;

            return fib = n =>
            {
                if(cache.ContainsKey(n))
                {
                    return cache[n];
                }
                
                if(n < 2)
                {
                    return n;
                }
                else
                {
                    count++;
                    var result = fib(n - 1) + fib(n - 2);
                    cache.Add(n, result);
                    return result;
                }
                
            };
        }

    }

    public class BinarySearchTree
    {
        public Node root;
        public int count;

        public BinarySearchTree()
        {
            root = null;
            count = 0;
        }

        public void Insert(int value)
        {
            if (root == null)
            {
                root = new Node(value);
                count++;
            }
            else
            {
                Node currentNode = root;

                while (true)
                {
                    if (value < currentNode.value)
                    {
                        if (currentNode.left != null)
                        {
                            if(value > currentNode.left.value)
                            {
                                Node newNode = new Node(value);
                                newNode.left = currentNode.left;
                                currentNode.left = newNode;
                                count++;
                                return;
                            }
                            else
                            {
                                currentNode = currentNode.left;
                            }
                        }
                        else
                        {
                            currentNode.left = new Node(value);
                            count++;
                            return;
                        }
                    }
                    else
                    {
                        if (currentNode.right != null)
                        {
                            if(value < currentNode.right.value)
                            {
                                Node newNode = new Node(value);
                                newNode.right = newNode;
                            }

                            currentNode = currentNode.right;
                        }
                        else
                        {
                            currentNode.right = new Node(value);
                            count++;
                            return;
                        }
                    }
                }
            }

        }

        public void Insert(List<int> values)
        {
            foreach (int value in values)
            {
                Insert(value);
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public List<int> BFS()
        {
            List<int> result = new List<int>();
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(root);

            while(queue.Count > 0)
            {
                Node currentNode = queue.Dequeue();
                result.Add(currentNode.value);
                Console.Write("{");

                if(currentNode.left != null)
                {
                    queue.Enqueue(currentNode.left);
                    Console.Write(currentNode.left.value);
                }

                Console.Write(", ");

                if(currentNode.right != null)
                {
                    queue.Enqueue(currentNode.right);
                    Console.Write(currentNode.right.value);
                }

                Console.WriteLine("}");
            }

            return result;
        }
    }

    public class Node
    {
        public int value;
        public Node left;
        public Node right;

        public Node(int value)
        {
            this.value = value;
            right = null;
            left = null;
        }
    }
}