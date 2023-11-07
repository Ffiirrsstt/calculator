using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace calculator
{
    public partial class Form1 : Form
    {
        void settingDataCalClick()
        {
            btnNum0.Click += new EventHandler(btn_Click);
            btnNum1.Click += new EventHandler(btn_Click);
            btnNum2.Click += new EventHandler(btn_Click);
            btnNum3.Click += new EventHandler(btn_Click);
            btnNum4.Click += new EventHandler(btn_Click);
            btnNum5.Click += new EventHandler(btn_Click);
            btnNum6.Click += new EventHandler(btn_Click);
            btnNum7.Click += new EventHandler(btn_Click);
            btnNum8.Click += new EventHandler(btn_Click);
            btnNum9.Click += new EventHandler(btn_Click);
            btnAdd.Click += new EventHandler(btn_Click);
            btnDif.Click += new EventHandler(btn_Click);
            btnDivide.Click += new EventHandler(btn_Click);
            btnMultiply.Click += new EventHandler(btn_Click);
            btnDot.Click += new EventHandler(btn_Click);

        }
            //btnRemove.Click += new EventHandler(btn_Click);

        //เมื่อกดปุ่มให้แสดงบนข้อมูล
        void btn_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                dataOutput.Text += button.Text;
                focusNext();
            }

        }

        //ให้เคอร์เซอร์อยู่ที่ตำแหน่งท้ายสุดของข้อความ (เพื่อที่จะพิมพ์ข้อมูลเข้าไปต่อ)
        void focusNext()
        {
            dataOutput.SelectionStart = dataOutput.Text.Length;
            dataOutput.Focus();
        }

        //เช็กว่าตัวแรกเป็นเครื่องหมายติดลบไหม อาทิ กรณี -5+6 , -10+5 เป็นต้น
        void firstNegative(string dataForCal,string[] signs, double[] dataNumbers)
        {
            if (dataForCal.StartsWith("-"))
            {
                //เนื่องจากมีเครื่องหมายลบอยู่ตัวแรก(หน้าตัวเลข) ดังนั้นจึงต้องนับเป็นติดลบ
                for (int i = 0; i < signs.Length - 1; i++)
                    signs[i] = signs[i + 1];
                Array.Resize(ref signs, signs.Length - 1);

                dataNumbers[0] = -1 * dataNumbers[0];
            }
        }

        //เกี่ยวกับค่าติดลบ อาทิ 5+-10 เป็นต้น
        //+- -- *- /-
        void negativeValue(string[] signs, double[] dataNumbers)
        {
            for (int count = 0; count < signs.Length; count++)
            {
                string fordata = signs[count];
                if (fordata == "+-" || fordata == "--" || fordata == "x-" || fordata == "÷-")
                {
                    dataNumbers[count + 1] = -1 * dataNumbers[count + 1];

                    if (fordata == "+-")
                        signs[count] = "+";
                    else if (fordata == "--")
                        signs[count] = "-";
                    else if (fordata == "x-")
                        signs[count] = "x";
                    else if (fordata == "÷-")
                        signs[count] = "÷";
                }
            }
        }

        // หาตำแหน่งของคูณหรือหารจากในกลุ่มเครื่องหมายทั้งหมด
        void indexofMultipAndDivis(string[] signs, List<int> dataIndexSigns)
        {
                for (int count = 0; count<signs.Length; count++)
                {
                    if (signs[count] == "x" || signs[count] == "÷")
                        dataIndexSigns.Add(count);
                }
        }

        string CommaInt(string dataForCal)
        {
            if (dataForCal[0]=='-')//ค่าติดลบ
            {
                dataForCal = dataForCal.Substring(1); //ตัดติดลบออก
                return "-"+(long.Parse(dataForCal, NumberStyles.AllowThousands).ToString("N0")); //ใส่ลบกลับคืน
            }    
            else
                return long.Parse(dataForCal, NumberStyles.AllowThousands).ToString("N0");
        }

        //แสดงผลลัพธ์ออกที่หน้าโปรแกรม
        //รูปแบบมี ,
        string outputResult(string dataForCal)
        {
            if (dataForCal.Contains("."))
            { //มีทศนิยม
                string[] str = dataForCal.Split('.');
                string strInt = str[0];
                strInt = CommaInt(strInt);
                return strInt + "." + str[1];
            }
            else return CommaInt(dataForCal); //ไม่มีทศนิยม
        }

        //คำนวณข้อมูล
        string calculate(List<int>  dataIndexSigns, double[] dataNumbers,String[] signs,string dataForCal)
        {
            /*
                   n n n+1
                   0 0 1
                   1 1 2
                   2 2 3
                   3 3 4
                */

            /* 5+9+7*8
            ได้ว่า ตัวเลข {5,9,7,8}
                เครื่องหมาย {"+","+","*"}
                ตำแหน่งเครื่องหมายคูณหรือหารในกลุ่มเครื่องหมาย คือ index 2
            ซึ่ง ต้องทำ 7*8 โดย
                7 คือ ตัวเลข index2
                8 คือ ตัวเลข index3
            ให้ ตำแหน่งเครื่องหมายคูณหรือหารในกลุ่มเครื่องหมาย คือ n [จาก index = 2]
                ข้อมูลชุดแรก คือ n [จาก index = 2]
                ข้อมูลชุดสอง คือ n+1 [จาก index = 3
                                เขียนว่า n+1 เมื่อ n = ตำแหน่งเครื่องหมายคูณหรือหารในกลุ่มเครื่องหมาย]
            */

            double data1, data2, resultParts;
            string strData, forSigns;
            if (dataIndexSigns.Count != 0) //ตรวจสอบว่ามีการใช้งานเครื่องหมายคูณหรือหารไหม
            {
                int indexOperators = dataIndexSigns[0]; //ลำดับความสำคัญของคูณและหารเท่ากัน ทำด้านซ้ายมาก่อน
                data1 = dataNumbers[indexOperators];
                data2 = dataNumbers[indexOperators + 1];

                forSigns = signs[indexOperators];
                strData = data1 + signs[indexOperators] + data2;
                resultParts = (forSigns == "x") ? data1 * data2 : data1 / data2;
                return dataForCal.Replace(strData, resultParts.ToString());
            }
            else //ลำดับความสำคัญของบวกและลบเท่ากัน ทำด้านซ้ายมาก่อน
            {
                data1 = dataNumbers[0];
                data2 = dataNumbers[1];
                forSigns = signs[0];
                strData = data1 + forSigns + data2;
                resultParts = (forSigns == "+") ? data1 + data2 : data1 - data2;
                return dataForCal.Replace(strData, resultParts.ToString());
            }
        }

        void Equal()
        {
            dataInput.Text = dataOutput.Text;
            string dataForCal = dataOutput.Text.Replace(",", ""); //กำจัดรูปแบบเลข 1,000 เป็นต้น

            //เคลียร์ค่า .0 เพราะจะทำให้มีปัญหาเวลาใส่ค่าใหม่แทนที่ในฟังก์ชันคำนวณ
            while (true)
            {
                int forDataIndexDotZero, indexNext;
                forDataIndexDotZero = dataForCal.IndexOf(".0");
                //forDataIndexDotZero = ตำแหน่งจุด
                //forDataIndexDotZero+1 = ตำแหน่ง 0
                indexNext = forDataIndexDotZero + 2;
                if (dataForCal.EndsWith(".0")) //ลงท้ายด้วย .0 ทำการทิ้งเลย เพราะไม่มีตัวเลขต่อท้ายแล้ว
                {
                    dataForCal = dataForCal.Substring(0, forDataIndexDotZero);
                }
                else if (forDataIndexDotZero!=-1)
                        {
                    //ตัด .0 ออก
                    //forDataIndexDotZero = -1 คือ ไม่มี .0 ใน dataForCal
                    dataForCal = dataForCal.Substring(0, forDataIndexDotZero)
                        + dataForCal.Substring(indexNext);
                        }
                else break;
            }

            while (true)
            {
                List<int> dataIndexSigns = new List<int>();

                //เก็บพวกสัญญาลักษณ์
                string[] signs = dataForCal.Split(new char[]
                { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9','.' },
                StringSplitOptions.RemoveEmptyEntries);

                //เก็บข้อมูลตัวเลข
                string[] parts = dataForCal.Split(new char[]
                { '+', '-', 'x', '÷'}, StringSplitOptions.RemoveEmptyEntries);
                double[] dataNumbers = Array.ConvertAll(parts, double.Parse);



                //เช็กว่ายังมีเครื่องหมายที่ยังต้องคำนวณอยู่หรือไม่
                //หรือถ้าหากยังมีเครื่องหมายคำนวณอยู่แล้วตัวเลขสำหรับการคำนวณเหลือเพียงชุดเดียวหรือไม่
                if (signs.Length == 0 || (signs.Length == 1 && dataNumbers.Length == 1))
                    break;

                //เช็กว่าตัวแรกเป็นเครื่องหมายติดลบไหม อาทิ กรณี -5+6 , -10+5 เป็นต้น
                firstNegative(dataForCal, signs, dataNumbers);


                //เกี่ยวกับค่าติดลบ อาทิ 5+-10 เป็นต้น
                //+- -- *- /-
                negativeValue(signs, dataNumbers);

                /*ลำดับความสำคัญ 
                    คูณและหารต้องทำก่อนบวกและลบ
                    - หาตำแหน่งของคูณหรือหารจากในกลุ่มเครื่องหมายทั้งหมด
                */
                indexofMultipAndDivis(signs, dataIndexSigns);

                    //คำนวณข้อมูล
                    //เก็บข้อมูลใหม่ที่คำนวณกลับไปที่ dataForCal
                    //หมายเหตุ : ข้างบนจะทำการเช็กว่าคำนวณครบหมดหรือยัง

                    /* dataForCal ในตอนแรก คือ 5+6*7
                       dataForCal เมื่อคำนวณข้อมูลใหม่ คือ 5+42
                       dataForCal คำนวณอีกครั้ง 47
                       dataForCal ไม่พบเครื่องหมายอีก ทำการออกจาก loop
                    */

                dataForCal = calculate(dataIndexSigns, dataNumbers, signs, dataForCal);
            }

            //แสดงผลลัพธ์
            dataOutput.Text = outputResult(dataForCal);

            focusNext();
        }

        //แทนที่ * ด้วย x และแทนที่ / ด้วย ÷
        void forReplaceMuAndDi()
        {
            dataOutput.Text = dataOutput.Text.Replace("*", "x");
            dataOutput.Text = dataOutput.Text.Replace("/", "÷");
        }

        //แปลงถ้าไม่ได้พิมพ์ 0. พิมพ์เพียง . ให้กลายเป็น 0.
        void strDot(string str, int length , char lastCharacter)
        {
            if (lastCharacter == '.')
            {
                char secondLastCharacter = length >= 2 ? str[str.Length - 2] : '\0';
                if (!char.IsDigit(secondLastCharacter)) { 
                   dataOutput.Text = str.Substring(0, length - 1) + "0.";
                   //อาทิ ถ้าเป็น +.
                   //เอา + ,ตัด . และเพิ่ม0.
                   //ได้ +0.
               }
            }
        }

        //แปลง 5.+ ให้เป็น 5.0+
        void strOperator(string str, int length, char lastCharacter)
        {
            if (lastCharacter == '+' || lastCharacter == '-' || lastCharacter == 'x' || lastCharacter == '÷')
            {
                char secondLastCharacter = length >= 2 ? str[str.Length - 2] : '\0';
                if (secondLastCharacter == '.')
                dataOutput.Text = str.Substring(0, length - 1) + "0" + lastCharacter;
            }
        }

         void strDataConvert(string str, char lastCharacter)
        {
            int indexEnd, indexStart;
            indexEnd = str.Length;
            indexStart = -1;

            if (lastCharacter != '.' && char.IsDigit(lastCharacter))
            {
                for (int count = indexEnd - 1; count >= 0; count--)
                {
                    char fordata = str[count];
                    //ถ้าไม่ใช่ตัวเลข (ยกเว้น .) ให้หยุด loop
                    if (char.IsDigit(fordata) || fordata == '.' || fordata == ',')
                        indexStart = count;
                    else break;
                }
                if (!(indexStart == -1)) //ไม่ใช่ตัวที่กำหนดเลยไม่มีการเปลี่ยนแปลงค่า indexStart
                {
                    string strNumber;
                    strNumber = str.Substring(indexStart, indexEnd - indexStart);
                    strNumber = outputResult(strNumber);//ทำให้อยู่ในรูปแบบ ,
                    dataOutput.Text = str.Substring(0, indexStart) + strNumber;
                }
            }
        }

        public Form1()
        {
            InitializeComponent();

            settingDataCalClick();

        }
        private void btnEqual_Click(object sender, EventArgs e)
        {
            Equal(); //คำนวณหาผลลัพธ์ของข้อมูล
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dataOutput.Text = "";
        }
        private void dataOutput_TextChanged(object sender, EventArgs e)
        {
            string str = dataOutput.Text;
            int length = str.Length;
            char lastCharacter;

            forReplaceMuAndDi();

            //พิมพ์ในนี้เพื่อให้สามารถใช้งานได้ทั้งในกรณีที่กดปุ่มบนหน้าโปรแกรม และกดคีย์บอร์ด

            if (str != "")
            {
                lastCharacter = str[length - 1];
                //แปลงถ้าไม่ได้พิมพ์ 0. พิมพ์เพียง . ให้กลายเป็น 0.
                strDot(str, length, lastCharacter);
                //แปลง 5.+ ให้เป็น 5.0+
                strOperator(str, length, lastCharacter);
                //แปลงตัวเลขให้มี ,
                strDataConvert(str, lastCharacter);
            }

            focusNext();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataOutput.Focus();
            this.ActiveControl = dataOutput;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            //ลบอักขระตัวสุดท้ายที่ถูกป้อนเข้าไปใน textbox
            int length = dataOutput.Text.Length-1;
            dataOutput.Text = dataOutput.Text.Substring(0, length);
        }

        private void data_Enter(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; //ไม่ให้เกิด Enter ใน TextBox
                Equal(); //คำนวณหาผลลัพธ์ของข้อมูล
            }
        }
    }
}
