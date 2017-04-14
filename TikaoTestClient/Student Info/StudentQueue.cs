using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TikaoTestThree
{
    class StudentQueue//学生队列管理
    {
        private Queue<StudentInfo> m_studentpool;
        public StudentQueue()
        {
            m_studentpool=new Queue<StudentInfo>();
        }

        public bool in_Queue(StudentInfo student)//入队
        {
            try
            {
                m_studentpool.Enqueue(student);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public StudentInfo out_Queue()//出队
        {
            if (m_studentpool.Count >= 0)
            {
                return m_studentpool.Dequeue();
            }
            return null;
        }
    }
}
