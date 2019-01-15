using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcealedSquare
{
    class AddTwoNumbers
    {
        static void xMain(string[] args)
        {
            ListNode head1 = new ListNode(2);
            head1.next = new ListNode(4);
            head1.next.next = new ListNode(3);

            ListNode head2 = new ListNode(5);
            head2.next = new ListNode(6);
            head2.next.next = new ListNode(4);

            ListNode result = Add2Numbers(head1, head2);
        }

        public static ListNode Add2Numbers(ListNode l1, ListNode l2)
        {
            int first = ConvertListNodeToInt(l1);
            int second = ConvertListNodeToInt(l2);

            return ConvertIntToListNode(first + second);
        }

        public static int ConvertListNodeToInt(ListNode list)
        {
            int value = 0;

            ListNode currentNode = list;
            int pow = 0;

            while (currentNode != null)
            {
                value += currentNode.val * (int)Math.Pow(10, pow);
                pow++;
                currentNode = currentNode.next;
            }

            return value;
        }

        private static ListNode ConvertIntToListNode(int num)
        {
            int num_of_digits = num.ToString().Length;
            int total = num % 10;
            ListNode head = new ListNode(total);
            ListNode currentNode = head;

            for(int i = 2; i <= num_of_digits; i++)
            {
                int pow = (int)Math.Pow(10, i);

                int remainder = num % pow;
                int nextValue = (remainder - total) * 10 / pow;
                ListNode nextNode = new ListNode(nextValue);
                total = remainder;
                currentNode.next = nextNode;
                currentNode = currentNode.next;
            }

            return head;
        }

    }

    public class ListNode
    {
      public int val;
      public ListNode next;
      public ListNode(int x) { val = x; }
    }
}
