using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudentInfo
{
    //
    //学生信息
    //
    class StudentInfo
    {
        
        public string name;//姓名
        public string idCard;//身份证
        public string sex;//性别
        public string number;//编号
        public string candNumber;//考生号
        public string contactNumber;//联系号码
        public string contactAddress;//联系地址
        public string candPhotos;//考生照片
        public string candFinger;//考生指纹
        public float score1;//考生成绩
        public float score2;
        public float score3;
        public float finalResult;//最后成绩
        public float foulsNumber;//犯规次数
        public float testsNumber;//已考次数

        public StudentInfo(string name, string idCard,string sex,string number,
            string candNumber,string contactNumber,string contactAddress ,string candPhotos,string candFinger)
        {
            this.name = name;
            this.idCard = idCard;
            this.sex = sex;
            this.number = number;
            this.candNumber = candNumber;
            this.contactNumber = contactNumber;
            this.contactAddress = contactAddress;
            this.candPhotos =candPhotos;
            this.candFinger = candFinger;
            this.score1 = 0;
            this.score2 = 0;
            this.score3 = 0;
            this.finalResult=0;//最后成绩
            this.foulsNumber=0;//犯规次数
            this.testsNumber=0;//已考次数
        }
    }
}
