using ARM;
using Roham.Services.Models;
using System.Text;

namespace Roham.Services.Services;

public class LocksService()
{
    private readonly ARMClass _axARMClass1 = new();
    private readonly Random _rand = new();
    private static byte[] _keyAES = new byte[16];
    private readonly int _nk_AES = 4;
    private readonly int _nb_AES = 4;
    private readonly int _nr_AES = 10;
    private readonly byte[,] _state_AES = new byte[4, 4];
    private readonly byte[,] _w_AES = new byte[4 * 11, 4];
    private readonly byte[] _temp_AES = new byte[4];
    private byte[] key_AES = new byte[16];

    #region variables

    byte[,] Sbox = new byte[16, 16]{
        {0x63, 0x7c, 0x77, 0x7b, 0xf2, 0x6b, 0x6f, 0xc5, 0x30, 0x01, 0x67, 0x2b, 0xfe, 0xd7, 0xab, 0x76},
        {0xca, 0x82, 0xc9, 0x7d, 0xfa, 0x59, 0x47, 0xf0, 0xad, 0xd4, 0xa2, 0xaf, 0x9c, 0xa4, 0x72, 0xc0},
        {0xb7, 0xfd, 0x93, 0x26, 0x36, 0x3f, 0xf7, 0xcc, 0x34, 0xa5, 0xe5, 0xf1, 0x71, 0xd8, 0x31, 0x15},
        {0x04, 0xc7, 0x23, 0xc3, 0x18, 0x96, 0x05, 0x9a, 0x07, 0x12, 0x80, 0xe2, 0xeb, 0x27, 0xb2, 0x75},
        {0x09, 0x83, 0x2c, 0x1a, 0x1b, 0x6e, 0x5a, 0xa0, 0x52, 0x3b, 0xd6, 0xb3, 0x29, 0xe3, 0x2f, 0x84},
        {0x53, 0xd1, 0x00, 0xed, 0x20, 0xfc, 0xb1, 0x5b, 0x6a, 0xcb, 0xbe, 0x39, 0x4a, 0x4c, 0x58, 0xcf},
        {0xd0, 0xef, 0xaa, 0xfb, 0x43, 0x4d, 0x33, 0x85, 0x45, 0xf9, 0x02, 0x7f, 0x50, 0x3c, 0x9f, 0xa8},
        {0x51, 0xa3, 0x40, 0x8f, 0x92, 0x9d, 0x38, 0xf5, 0xbc, 0xb6, 0xda, 0x21, 0x10, 0xff, 0xf3, 0xd2},
        {0xcd, 0x0c, 0x13, 0xec, 0x5f, 0x97, 0x44, 0x17, 0xc4, 0xa7, 0x7e, 0x3d, 0x64, 0x5d, 0x19, 0x73},
        {0x60, 0x81, 0x4f, 0xdc, 0x22, 0x2a, 0x90, 0x88, 0x46, 0xee, 0xb8, 0x14, 0xde, 0x5e, 0x0b, 0xdb},
        {0xe0, 0x32, 0x3a, 0x0a, 0x49, 0x06, 0x24, 0x5c, 0xc2, 0xd3, 0xac, 0x62, 0x91, 0x95, 0xe4, 0x79},
        {0xe7, 0xc8, 0x37, 0x6d, 0x8d, 0xd5, 0x4e, 0xa9, 0x6c, 0x56, 0xf4, 0xea, 0x65, 0x7a, 0xae, 0x08},
        {0xba, 0x78, 0x25, 0x2e, 0x1c, 0xa6, 0xb4, 0xc6, 0xe8, 0xdd, 0x74, 0x1f, 0x4b, 0xbd, 0x8b, 0x8a},
        {0x70, 0x3e, 0xb5, 0x66, 0x48, 0x03, 0xf6, 0x0e, 0x61, 0x35, 0x57, 0xb9, 0x86, 0xc1, 0x1d, 0x9e},
        {0xe1, 0xf8, 0x98, 0x11, 0x69, 0xd9, 0x8e, 0x94, 0x9b, 0x1e, 0x87, 0xe9, 0xce, 0x55, 0x28, 0xdf},
        {0x8c, 0xa1, 0x89, 0x0d, 0xbf, 0xe6, 0x42, 0x68, 0x41, 0x99, 0x2d, 0x0f, 0xb0, 0x54, 0xbb, 0x16}
     };
    byte[,] iSbox = new byte[16, 16]{
        {0x52, 0x09, 0x6a, 0xd5, 0x30, 0x36, 0xa5, 0x38, 0xbf, 0x40, 0xa3, 0x9e, 0x81, 0xf3, 0xd7, 0xfb},
        {0x7c, 0xe3, 0x39, 0x82, 0x9b, 0x2f, 0xff, 0x87, 0x34, 0x8e, 0x43, 0x44, 0xc4, 0xde, 0xe9, 0xcb},
        {0x54, 0x7b, 0x94, 0x32, 0xa6, 0xc2, 0x23, 0x3d, 0xee, 0x4c, 0x95, 0x0b, 0x42, 0xfa, 0xc3, 0x4e},
        {0x08, 0x2e, 0xa1, 0x66, 0x28, 0xd9, 0x24, 0xb2, 0x76, 0x5b, 0xa2, 0x49, 0x6d, 0x8b, 0xd1, 0x25},
        {0x72, 0xf8, 0xf6, 0x64, 0x86, 0x68, 0x98, 0x16, 0xd4, 0xa4, 0x5c, 0xcc, 0x5d, 0x65, 0xb6, 0x92},
        {0x6c, 0x70, 0x48, 0x50, 0xfd, 0xed, 0xb9, 0xda, 0x5e, 0x15, 0x46, 0x57, 0xa7, 0x8d, 0x9d, 0x84},
        {0x90, 0xd8, 0xab, 0x00, 0x8c, 0xbc, 0xd3, 0x0a, 0xf7, 0xe4, 0x58, 0x05, 0xb8, 0xb3, 0x45, 0x06},
        {0xd0, 0x2c, 0x1e, 0x8f, 0xca, 0x3f, 0x0f, 0x02, 0xc1, 0xaf, 0xbd, 0x03, 0x01, 0x13, 0x8a, 0x6b},
        {0x3a, 0x91, 0x11, 0x41, 0x4f, 0x67, 0xdc, 0xea, 0x97, 0xf2, 0xcf, 0xce, 0xf0, 0xb4, 0xe6, 0x73},
        {0x96, 0xac, 0x74, 0x22, 0xe7, 0xad, 0x35, 0x85, 0xe2, 0xf9, 0x37, 0xe8, 0x1c, 0x75, 0xdf, 0x6e},
        {0x47, 0xf1, 0x1a, 0x71, 0x1d, 0x29, 0xc5, 0x89, 0x6f, 0xb7, 0x62, 0x0e, 0xaa, 0x18, 0xbe, 0x1b},
        {0xfc, 0x56, 0x3e, 0x4b, 0xc6, 0xd2, 0x79, 0x20, 0x9a, 0xdb, 0xc0, 0xfe, 0x78, 0xcd, 0x5a, 0xf4},
        {0x1f, 0xdd, 0xa8, 0x33, 0x88, 0x07, 0xc7, 0x31, 0xb1, 0x12, 0x10, 0x59, 0x27, 0x80, 0xec, 0x5f},
        {0x60, 0x51, 0x7f, 0xa9, 0x19, 0xb5, 0x4a, 0x0d, 0x2d, 0xe5, 0x7a, 0x9f, 0x93, 0xc9, 0x9c, 0xef},
        {0xa0, 0xe0, 0x3b, 0x4d, 0xae, 0x2a, 0xf5, 0xb0, 0xc8, 0xeb, 0xbb, 0x3c, 0x83, 0x53, 0x99, 0x61},
        {0x17, 0x2b, 0x04, 0x7e, 0xba, 0x77, 0xd6, 0x26, 0xe1, 0x69, 0x14, 0x63, 0x55, 0x21, 0x0c, 0x7d}
     };
    byte[] Rcon1 = new byte[11] { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36 };

    #endregion

    public LockViewModel CheckLock()
    {
        try
        {
            uint Rand1;
            byte bteRes = 0;
            object obj = new();
            byte[] bte1 = new byte[16];
            byte bb = 0;
            byte[] bteRand = new byte[16];

            while (bteRes != 1 && bteRes != 101 && bteRes != 106 && bteRes != 113)
            {
                Rand1 = (uint)_rand.Next(2147483647);
                obj = _axARMClass1.Authenticate(Rand1);
                bteRes = CheckAuthenticate(obj, Rand1);
            }

            if (bteRes == 1)
            {
                _keyAES = Encoding.Default.GetBytes("SecureR@ham14032");
                bte1 = CreateUserKey("BCC32F193985EDEDC65685F35E493A10", _keyAES);
                _axARMClass1.FindFirstARM(
                    bte1,
                    "02E71E17BB7139549D48BCC51731972B6C95B630555AE035257FB2C1469C9B157A58023B3C876173F29DE2E65B0BDD67",
                    "562F925E11C7F2959986E4A887D4F28D9101B869731D3C0C5A22B48F326F1DB40A883BFC45763D2DC9A7DBE7993A683D");
                for (int i = 0; i < 16; i++)
                    bteRand[i] = (byte)_rand.Next(255);
                obj = _axARMClass1.GetARMErrorCode(bteRand);
                bb = GenerateErrorCode(obj, _keyAES, bteRand);
                if (bb == 100)
                {
                    uint RProg = (uint)_rand.Next(2147483647);
                    var seial_number_obj = _axARMClass1.GetARMData(IWhichData.SERIAL_NUM, RProg);
                    var int_val_1_obj = _axARMClass1.GetARMData(IWhichData.INT_VAL1, RProg);

                    var serial_number = DecodData(seial_number_obj, IWhichData.SERIAL_NUM, RProg);
                    var int_val_1 = DecodData(int_val_1_obj, IWhichData.INT_VAL1, RProg);

                    Console.WriteLine("Lock Succeeded.");
                    return new LockViewModel { 
                        IsExist = true,
                        SerialNumber = serial_number,
                        Value = int_val_1
                    };
                }
                //if (bb != 100)
                else
                {
                    //Console.Beep(5000, 5000);
                    Console.WriteLine("Lock Not Found!");
                    //throw new Exception("Lock Error!");
                    return new LockViewModel();
                }
            }
            else
            {
                //Console.Beep(5000, 5000);
                Console.WriteLine("Lock Not Found!");
                //throw new Exception("Lock Error!");
                return new LockViewModel();
            }
        }
        catch (Exception exp)
        {
            Console.Beep(5000, 5000);
            Console.WriteLine("Lock Not Found!");
            return new LockViewModel();
        }
        //else if (bteRes == 113)
        //	throw new Exception("Error!");
        //else if (bteRes == 101)
        //	throw new Exception("Lock Not Found!");
        //else if (bteRes == 106)
        //	throw new Exception("Error In Send Receive!");
    }

    private byte CheckAuthenticate(object obj, uint Param1)
    {
        byte[] bte2 = new byte[40];
        object obj1 = new();
        byte Result = 0;
        bte2 = (byte[])obj;
        double P1 = 0, P2 = 0, P3 = 0;
        uint y = 0;
        try
        {
            if (bte2[0] == 0 && bte2[1] == 0 && bte2[2] == 0 && bte2[13] == 0 && bte2[13] == 0 && bte2[15] == 106)
                Result = 106;
            else if (bte2[0] == 0 && bte2[1] == 0 && bte2[2] == 0 && bte2[13] == 0 && bte2[13] == 0 && bte2[15] == 101)
                Result = 101;
            else
            {
                P1 = BitConverter.ToDouble(bte2, 0);
                P2 = BitConverter.ToDouble(bte2, 8);
                P3 = BitConverter.ToDouble(bte2, 16);
                y = FuncBig(P1, P2, P3);
                if (y == Param1)
                    Result = 1;
                else if (y == 0)
                    Result = 101;
                else if (y == 25279)
                    Result = 120;
                else
                    Result = 113;
            }
        }
        catch
        {
        }
        return Result;
    }

    private uint FuncBig(double p, double k, double z)
    {
        double a1, a2, a3, a11, a12, a13, a14, a15, a16, a17, a18, a19;
        uint y = 0;
        try
        {
            a11 = +(16384 * p - 50139) / 262144 * (85 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5, 0.6666666666666667) *
                    Math.Pow(-17022339072 * k
                    * k + 15252992 * Math.Sqrt(1245456 * k * k * k * k - 13270041 * k * k * k
                    + 23218866 * k * k - 14741196 * k + 3193816) + 634588984896 * k -
                    4081387131107, 0.3333333333333333) / 1906624) * (+(40368 * z - 175789) /
                    585336);
            a12 = 49 * Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984
                   * p * p * p * p - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p +
                   1531016) + 22179889152 * p - 27017267233, 0.3333333333333333) / 786432 * (+(5
                   * (184512 * k - 3439283)) / 5719872) * (53 * Math.Pow(-2444363136 * z *
                   z + 1560896 * Math.Sqrt(2452356 * z * z * z * z - 10559295 * z * z * z + 14578974 * z * z -
                   8266068 * z + 1675432) + 21288751056 * z - 35270478217, 0.3333333333333333) / 585336);
            a13 = -(8431152758784 * p - 15711066875385) / (28092137472 *
                 Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p * p * p * p
                 - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p + 1531016) +
                 22179889152 * p - 27017267233, 0.3333333333333333)) * (-(22303482820560000 * k
                 - 215301704280305625) / (1063295605440 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5,
                 0.6666666666666667) * Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k
                 * k * k * k - 13270041 * k * k * k + 23218866 * k * k - 14741196 * k
                 + 3193816) + 634588984896 * k - 4081387131107, 0.3333333333333333))) * (53 *
                 Math.Pow(-2444363136 * z * z + 1560896 * Math.Sqrt(2452356 * z *
                 z * z * z - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z +
                 1675432) + 21288751056 * z - 35270478217, 0.3333333333333333) / 585336);
            a14 = 49 * Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p *
                 p * p * p - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p +
                 1531016) + 22179889152 * p - 27017267233, 0.3333333333333333) / 786432 *
                 (-(22303482820560000 * k - 215301704280305625) / (1063295605440 * Math.Pow(3, 0.3333333333333333) *
                 Math.Pow(5, 0.6666666666666667) * Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k
                 * k * k * k - 13270041 * k * k * k + 23218866 * k * k - 14741196 * k
                 + 3193816) + 634588984896 * k - 4081387131107, 0.3333333333333333))) * (-
                 (8762385701088 * z - 21999409730433) / (22615627032 * Math.Pow(-2444363136 * z * z +
                 1560896 * Math.Sqrt(2452356 * z * z * z * z - 10559295 * z * z * z + 14578974 * z * z -
                 8266068 * z + 1675432) + 21288751056 * z - 35270478217, 0.3333333333333333)));
            a15 = 49 * Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p *
                  p * p * p - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p +
                  1531016) + 22179889152 * p - 27017267233, 0.3333333333333333) / 786432
                  * (85 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5, 0.6666666666666667) *
                  Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k * k * k * k -
                  13270041 * k * k * k + 23218866 * k * k - 14741196 * k + 3193816) + 634588984896 * k -
                  4081387131107, 0.3333333333333333) / 1906624) * (+(40368 * z - 175789) / 585336);
            a16 = -(8431152758784 * p - 15711066875385) / (28092137472 *
                 Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p * p * p * p
                 - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p + 1531016) +
                 22179889152 * p - 27017267233, 0.3333333333333333)) * (+(5 *
                 (184512 * k - 3439283)) / 5719872) * (+(40368 * z - 175789) / 585336);
            a17 = -(8431152758784 * p - 15711066875385) / (28092137472 *
                   Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p * p * p * p
                   - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p + 1531016) +
                   22179889152 * p - 27017267233, 0.3333333333333333)) * (+(5 *
                   (184512 * k - 3439283)) / 5719872) * (53 * Math.Pow(-2444363136 * z *
                   z + 1560896 * Math.Sqrt(2452356 * z * z * z * z - 10559295 * z * z * z + 14578974 * z * z
                   - 8266068 * z + 1675432) + 21288751056 * z - 35270478217, 0.3333333333333333) / 585336);
            a18 = +(16384 * p - 50139) /
                 262144 * (85 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5, 0.6666666666666667) *
                 Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k * k * k * k - 13270041 * k * k * k
                 + 23218866 * k * k - 14741196 * k + 3193816) + 634588984896 * k -
                 4081387131107, 0.3333333333333333) / 1906624) * (53 * Math.Pow(-2444363136 * z
                 * z + 1560896 * Math.Sqrt(2452356 * z * z * z * z - 10559295 * z * z * z + 14578974 * z * z -
                 8266068 * z + 1675432) + 21288751056 * z - 35270478217, 0.3333333333333333) / 585336);
            a19 = +(16384 * p - 50139) / 262144 * (-(22303482820560000 * k -
                  215301704280305625) / (1063295605440 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5,
                  0.6666666666666667) * Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k
                  * k * k * k - 13270041 * k * k * k + 23218866 * k * k - 14741196 * k
                  + 3193816) + 634588984896 * k - 4081387131107, 0.3333333333333333))) * (-
                  (8762385701088 * z - 21999409730433) / (22615627032 * Math.Pow(-2444363136 * z * z +
                  1560896 * Math.Sqrt(2452356 * z * z * z * z - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z +
                  1675432) + 21288751056 * z - 35270478217, 0.3333333333333333)));
            a1 = a11 + a12 + a13 + a14 + a15 + a16 + a17 + a18 + a19;

            a2 = +(16384 * p - 50139) / 262144 * (+(5 * (184512 * k - 3439283)) / 5719872) *
                (53 * Math.Pow(-2444363136 * z * z + 1560896 * Math.Sqrt(2452356 * z *
              z * z * z - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z +
              1675432) + 21288751056 * z - 35270478217, 0.3333333333333333) / 585336) + 49
              * Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p *
              p * p * p - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p +
              1531016) + 22179889152 * p - 27017267233, 0.3333333333333333) / 786432 * (+(5
              * (184512 * k - 3439283)) / 5719872) * (-(8762385701088 * z - 21999409730433) / (22615627032 *
              Math.Pow(-2444363136 * z * z + 1560896 * Math.Sqrt(2452356 * z * z * z * z
              - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z + 1675432) +
              21288751056 * z - 35270478217, 0.3333333333333333))) + (+(16384 * p - 50139) /
              262144) * (85 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5, 0.6666666666666667) *
              Math.Pow(-17022339072 * k
              * k + 15252992 * Math.Sqrt(1245456 * k * k * k * k - 13270041 * k * k * k
              + 23218866 * k * k - 14741196 * k + 3193816) + 634588984896 * k -
              4081387131107, 0.3333333333333333) / 1906624) * (-
              (8762385701088 * z - 21999409730433) / (22615627032 *
              Math.Pow(-2444363136 * z * z + 1560896 * Math.Sqrt(2452356 * z * z * z * z
              - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z + 1675432) +
              21288751056 * z - 35270478217, 0.3333333333333333))) + (+(16384 * p - 50139) /
              262144) * (+(5 * (184512 * k - 3439283)) / 5719872) * (-
              (8762385701088 * z - 21999409730433) / (22615627032 *
              Math.Pow(-2444363136 * z * z + 1560896 * Math.Sqrt(2452356 * z * z * z * z
              - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z + 1675432) +
              21288751056 * z - 35270478217, 0.3333333333333333))) + 49 *
              Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p *
              p * p * p - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p +
              1531016) + 22179889152 * p - 27017267233, 0.3333333333333333) / 786432
              * (85 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5, 0.6666666666666667) *
              Math.Pow(-17022339072 * k
              * k + 15252992 * Math.Sqrt(1245456 * k * k * k * k - 13270041 * k * k * k
              + 23218866 * k * k - 14741196 * k + 3193816) + 634588984896 * k -
              4081387131107, 0.3333333333333333) / 1906624) * (53 * Math.Pow(-2444363136 * z
              * z + 1560896 * Math.Sqrt(2452356 * z *
              z * z * z - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z +
              1675432) + 21288751056 * z - 35270478217, 0.3333333333333333) / 585336) + 49
              * Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p *
              p * p * p - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p +
              1531016) + 22179889152 * p - 27017267233, 0.3333333333333333) / 786432 * (85
              * Math.Pow(3, 0.3333333333333333) * Math.Pow(5, 0.6666666666666667) *
              Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k * k * k * k - 13270041 * k * k * k
              + 23218866 * k * k - 14741196 * k + 3193816) + 634588984896 * k -
              4081387131107, 0.3333333333333333) / 1906624) * (-(8762385701088 * z - 21999409730433) / (22615627032 *
              Math.Pow(-2444363136 * z * z + 1560896 * Math.Sqrt(2452356 * z * z * z * z
              - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z + 1675432) +
              21288751056 * z - 35270478217, 0.3333333333333333))) + -
              (8431152758784 * p - 15711066875385) / (28092137472 *
              Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p * p * p * p
              - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p + 1531016) +
              22179889152 * p - 27017267233, 0.3333333333333333)) * (85 * Math.Pow(3, 0.3333333333333333)
              * Math.Pow(5, 0.6666666666666667) * Math.Pow(-17022339072 * k
              * k + 15252992 * Math.Sqrt(1245456 * k * k * k * k - 13270041 * k * k * k
              + 23218866 * k * k - 14741196 * k + 3193816) + 634588984896 * k -
              4081387131107, 0.3333333333333333) / 1906624) * (-
              (8762385701088 * z - 21999409730433) / (22615627032 *
              Math.Pow(-2444363136 * z * z + 1560896 * Math.Sqrt(2452356 * z * z * z * z
              - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z + 1675432) +
              21288751056 * z - 35270478217, 0.3333333333333333))) + -
              (8431152758784 * p - 15711066875385) / (28092137472 *
              Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p * p * p * p
              - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p + 1531016) +
              22179889152 * p - 27017267233, 0.3333333333333333)) * (-(22303482820560000 * k
              - 215301704280305625) / (1063295605440 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5,
              0.6666666666666667) * Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k
              * k * k * k - 13270041 * k * k * k + 23218866 * k * k - 14741196 * k
              + 3193816) + 634588984896 * k - 4081387131107, 0.3333333333333333))) * (-
              (8762385701088 * z - 21999409730433) / (22615627032 *
              Math.Pow(-2444363136 * z * z + 1560896 * Math.Sqrt(2452356 * z * z * z * z
              - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z + 1675432) +
              21288751056 * z - 35270478217, 0.3333333333333333))) + -
              (8431152758784 * p - 15711066875385) / (28092137472 * Math.Pow(-3623878656 * p * p + 2097152 *
              Math.Sqrt(2985984 * p * p * p * p - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p + 1531016) +
              22179889152 * p - 27017267233, 0.3333333333333333)) * (-(22303482820560000 * k
              - 215301704280305625) / (1063295605440 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5,
              0.6666666666666667) * Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k
              * k * k * k - 13270041 * k * k * k + 23218866 * k * k - 14741196 * k
              + 3193816) + 634588984896 * k - 4081387131107, 0.3333333333333333))) * (+(40368 * z - 175789) / 585336);
            a3 = -(8431152758784 * p - 15711066875385) / (28092137472 *
              Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p * p * p * p
              - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p + 1531016) +
              22179889152 * p - 27017267233, 0.3333333333333333)) * (85 * Math.Pow(3, 0.3333333333333333)
              * Math.Pow(5, 0.6666666666666667) * Math.Pow(-17022339072 * k
              * k + 15252992 * Math.Sqrt(1245456 * k * k * k * k - 13270041 * k * k * k
              + 23218866 * k * k - 14741196 * k + 3193816) + 634588984896 * k -
              4081387131107, 0.3333333333333333) / 1906624) * (+(40368 * z - 175789) /
              585336) + 49 * Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984
              * p * p * p * p - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p +
              1531016) + 22179889152 * p - 27017267233, 0.3333333333333333) / 786432 *
              (-(22303482820560000 * k -
              215301704280305625) / (1063295605440 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5,
              0.6666666666666667) * Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k
              * k * k * k - 13270041 * k * k * k + 23218866 * k * k - 14741196 * k
              + 3193816) + 634588984896 * k - 4081387131107, 0.3333333333333333))) * (+(40368
              * z - 175789) / 585336) + (+(16384 * p - 50139) / 262144) * (-(22303482820560000 * k -
              215301704280305625) / (1063295605440 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5,
              0.6666666666666667) * Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k
              * k * k * k - 13270041 * k * k * k + 23218866 * k * k - 14741196 * k
              + 3193816) + 634588984896 * k - 4081387131107, 0.3333333333333333))) * (+(40368
              * z - 175789) / 585336) + (+(16384 * p - 50139) / 262144) * (+(5 *
              (184512 * k - 3439283)) / 5719872) * (+(40368 * z - 175789) /
              585336) + (+(16384 * p - 50139) / 262144) * (-(22303482820560000 * k -
              215301704280305625) / (1063295605440 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5,
              0.6666666666666667) * Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k
              * k * k * k - 13270041 * k * k * k + 23218866 * k * k - 14741196 * k
              + 3193816) + 634588984896 * k - 4081387131107, 0.3333333333333333))) * (53 *
              Math.Pow(-2444363136 * z * z + 1560896 * Math.Sqrt(2452356 * z *
              z * z * z - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z +
              1675432) + 21288751056 * z - 35270478217, 0.3333333333333333) / 585336) + 49
              * Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p *
              p * p * p - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p +
              1531016) + 22179889152 * p - 27017267233, 0.3333333333333333) / 786432 *
              (-(22303482820560000 * k - 215301704280305625) / (1063295605440 * Math.Pow(3, 0.3333333333333333) * Math.Pow(5,
              0.6666666666666667) * Math.Pow(-17022339072 * k * k + 15252992 * Math.Sqrt(1245456 * k
              * k * k * k - 13270041 * k * k * k + 23218866 * k * k - 14741196 * k
              + 3193816) + 634588984896 * k - 4081387131107, 0.3333333333333333))) * (53 *
              Math.Pow(-2444363136 * z * z + 1560896 * Math.Sqrt(2452356 * z *
              z * z * z - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z +
              1675432) + 21288751056 * z - 35270478217, 0.3333333333333333) / 585336) + 49
              * Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p *
              p * p * p - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p +
              1531016) + 22179889152 * p - 27017267233, 0.3333333333333333) / 786432 * (+(5
              * (184512 * k - 3439283)) / 5719872) * (+(40368 * z - 175789) /
              585336) + -(8431152758784 * p - 15711066875385) / (28092137472 *
              Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p * p * p * p
              - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p + 1531016) +
              22179889152 * p - 27017267233, 0.3333333333333333)) * (85 * Math.Pow(3, 0.3333333333333333)
              * Math.Pow(5, 0.6666666666666667) * Math.Pow(-17022339072 * k
              * k + 15252992 * Math.Sqrt(1245456 * k * k * k * k - 13270041 * k * k * k
              + 23218866 * k * k - 14741196 * k + 3193816) + 634588984896 * k -
              4081387131107, 0.3333333333333333) / 1906624) * (53 * Math.Pow(-2444363136 * z
              * z + 1560896 * Math.Sqrt(2452356 * z * z * z * z - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z +
              1675432) + 21288751056 * z - 35270478217, 0.3333333333333333) / 585336) + -
              (8431152758784 * p - 15711066875385) / (28092137472 *
              Math.Pow(-3623878656 * p * p + 2097152 * Math.Sqrt(2985984 * p * p * p * p
              - 11139147 * p * p * p + 14315670 * p * p - 7774308 * p + 1531016) +
              22179889152 * p - 27017267233, 0.3333333333333333)) * (+(5 *
              (184512 * k - 3439283)) / 5719872) * (-(8762385701088 * z - 21999409730433) / (22615627032 *
              Math.Pow(-2444363136 * z * z + 1560896 * Math.Sqrt(2452356 * z * z * z * z
              - 10559295 * z * z * z + 14578974 * z * z - 8266068 * z + 1675432) +
              21288751056 * z - 35270478217, 0.3333333333333333)));
            y = (uint)Math.Round(Math.Pow(a1 + a2 + a3, 0.3333333333333333));
        }
        catch { }
        return y;
    }

    private byte[] CreateUserKey(string str, byte[] Key)
    {
        byte[] bte1 = new byte[16];
        byte[] bte2 = new byte[16];
        try
        {
            if (str.Length != 32)
                for (int i = 0; i < 16; i++)
                {
                    bte1[i] = 0;
                }
            else
            {
                bte2 = HexStringToByteArray(str);
                AES(bte2, bte1, Key);
            }
        }
        catch { }
        return bte1;
    }

    private byte[] HexStringToByteArray(string hex)
    {
        string str;
        byte[] a = new byte[16];
        try
        {
            for (int i = 0; i < 16; i++)
            {
                str = hex.Substring(i * 2, 2);
                a[i] = (byte)Convert.ToInt32(str, 16);
            }
        }
        catch
        {
        }
        return a;
    }

    private void AES(byte[] input, byte[] output, byte[] Key)  // encipher 16-bit input
    {
        int i, round;
        try
        {
            KeyExpansion(Key);
            for (i = 0; i < 4 * 4; i++)
            {
                _state_AES[i % 4, i / 4] = input[i];
            }
            AddRoundKey(0);
            for (round = 1; round <= _nr_AES - 1; ++round)  // main round loop
            {
                SubBytes();
                ShiftRows();
                MixColumns();
                AddRoundKey(round);
            }  // main round loop
            SubBytes();
            ShiftRows();
            AddRoundKey(_nr_AES);
            // output = state
            for (i = 0; i < 4 * 4; ++i)
            {
                output[i] = _state_AES[i % 4, i / 4];
            }
            return;
        }
        catch
        {
        }
    }

    private void KeyExpansion(byte[] AESKEY)
    {
        int row;
        for (row = 0; row < _nk_AES; ++row)
        {
            _w_AES[row, 0] = AESKEY[4 * row];
            _w_AES[row, 1] = AESKEY[4 * row + 1];
            _w_AES[row, 2] = AESKEY[4 * row + 2];
            _w_AES[row, 3] = AESKEY[4 * row + 3];
        }
        for (row = _nk_AES; row < _nb_AES * (_nr_AES + 1); ++row)
        {
            _temp_AES[0] = _w_AES[row - 1, 0];
            _temp_AES[1] = _w_AES[row - 1, 1];
            _temp_AES[2] = _w_AES[row - 1, 2];
            _temp_AES[3] = _w_AES[row - 1, 3];
            if (row % _nk_AES == 0)
            {
                RotWord();
                SubWord();
                _temp_AES[0] = (byte)(_temp_AES[0] ^ Rcon1[row / _nk_AES]);
            }
            _w_AES[row, 0] = (byte)(_w_AES[row - _nk_AES, 0] ^ _temp_AES[0]);
            _w_AES[row, 1] = (byte)(_w_AES[row - _nk_AES, 1] ^ _temp_AES[1]);
            _w_AES[row, 2] = (byte)(_w_AES[row - _nk_AES, 2] ^ _temp_AES[2]);
            _w_AES[row, 3] = (byte)(_w_AES[row - _nk_AES, 3] ^ _temp_AES[3]);
        }  // for loop
        return;
    }

    private void RotWord()
    {
        byte[] result = [_temp_AES[1], _temp_AES[2], _temp_AES[3], _temp_AES[0]];
        _temp_AES[0] = result[0];
        _temp_AES[1] = result[1];
        _temp_AES[2] = result[2];
        _temp_AES[3] = result[3];
        return;
    }
    private void SubWord()
    {
        _temp_AES[0] = Sbox[_temp_AES[0] >> 4, _temp_AES[0] & 0x0f];
        _temp_AES[1] = Sbox[_temp_AES[1] >> 4, _temp_AES[1] & 0x0f];
        _temp_AES[2] = Sbox[_temp_AES[2] >> 4, _temp_AES[2] & 0x0f];
        _temp_AES[3] = Sbox[_temp_AES[3] >> 4, _temp_AES[3] & 0x0f];
        return;
    }

    private void AddRoundKey(int round)
    {

        for (int r = 0; r < 4; ++r)
        {
            for (int c = 0; c < 4; ++c)
            {
                _state_AES[r, c] = (byte)(_state_AES[r, c] ^ _w_AES[round * 4 + c, r]);
            }
        }
        return;
    }
    private void SubBytes()
    {
        for (int r = 0; r < 4; ++r)
        {
            for (int c = 0; c < 4; ++c)
            {
                _state_AES[r, c] = Sbox[_state_AES[r, c] >> 4, _state_AES[r, c] & 0x0f];
            }
        }
        return;
    }
    private void ShiftRows()
    {
        byte[,] temp1 = new byte[4, 4];
        for (int r = 0; r < 4; ++r)  // copy State into temp1[]
        {
            for (int c = 0; c < 4; ++c)
            {
                temp1[r, c] = _state_AES[r, c];
            }
        }
        for (int r = 1; r < 4; ++r)  // shift temp1 into State
        {
            for (int c = 0; c < 4; ++c)
            {
                _state_AES[r, c] = temp1[r, (c + r) % 4];  //State_AES[r][c] = temp1[ r][ (c + r) % Nb_AES ];
            }
        }
        return;
    }
    private void MixColumns()
    {
        byte[,] temp1 = new byte[4, 4];
        for (int r = 0; r < 4; ++r)  // copy State into temp1[]
        {
            for (int c = 0; c < 4; ++c)
            {
                temp1[r, c] = _state_AES[r, c];
            }
        }
        for (int c = 0; c < 4; ++c)
        {
            _state_AES[0, c] = (byte)(gfmultby02(temp1[0, c]) ^ gfmultby03(temp1[1, c]) ^ gfmultby01(temp1[2, c]) ^ gfmultby01(temp1[3, c]));
            _state_AES[1, c] = (byte)(gfmultby01(temp1[0, c]) ^ gfmultby02(temp1[1, c]) ^ gfmultby03(temp1[2, c]) ^ gfmultby01(temp1[3, c]));
            _state_AES[2, c] = (byte)(gfmultby01(temp1[0, c]) ^ gfmultby01(temp1[1, c]) ^ gfmultby02(temp1[2, c]) ^ gfmultby03(temp1[3, c]));
            _state_AES[3, c] = (byte)(gfmultby03(temp1[0, c]) ^ gfmultby01(temp1[1, c]) ^ gfmultby01(temp1[2, c]) ^ gfmultby02(temp1[3, c]));
        }
        return;
    }

    private byte gfmultby01(byte b)
    {
        return b;
    }
    private byte gfmultby02(byte b)
    {
        byte bte;

        if (b < 0x80)
            bte = (byte)(b << 1);
        else
            bte = (byte)(b << 1 ^ 0x1b);
        return bte;
    }
    private byte gfmultby03(byte b)
    {
        byte bte;
        bte = (byte)(gfmultby02(b) ^ b);
        return bte;
    }
    private byte gfmultby09(byte b)
    {
        byte bte;
        bte = (byte)(gfmultby02(gfmultby02(gfmultby02(b))) ^ b);
        return bte;
    }
    private byte gfmultby0b(byte b)
    {
        byte bte;
        bte = (byte)(gfmultby02(gfmultby02(gfmultby02(b))) ^ gfmultby02(b) ^ b);
        return bte;
    }
    private byte gfmultby0d(byte b)
    {
        byte bte;
        bte = (byte)(gfmultby02(gfmultby02(gfmultby02(b))) ^ gfmultby02(gfmultby02(b)) ^ b);
        return bte;
    }

    private byte gfmultby0e(byte b)
    {
        byte bte;
        bte = (byte)(gfmultby02(gfmultby02(gfmultby02(b))) ^ gfmultby02(gfmultby02(b)) ^ gfmultby02(b));
        return bte;
    }

    private byte GenerateErrorCode(object OBJ, byte[] Key, byte[] bterand)
    {
        byte[] Finalbte16 = new byte[16];
        byte[] bte16 = new byte[16];
        byte bb = 0;
        byte[] bteAnswer = new byte[16];
        bte16 = (byte[])OBJ;
        try
        {
            if (bte16[0] == 0 && bte16[1] == 0 && bte16[2] == 0)
            {
                bb = bte16[15];
            }
            else
            {
                ReverseAES(bte16, ref bteAnswer, Key);
                if (bteAnswer[0] == bterand[0] && bteAnswer[1] == bterand[1] && bteAnswer[2] == bterand[2] && bteAnswer[3] == bterand[3] && bteAnswer[4] == bterand[4])
                {
                    bb = bteAnswer[15];
                }
                else
                    bb = 200;
            }
        }
        catch { }
        return bb;
    }

    private void ReverseAES(byte[] bteInput, ref byte[] output, byte[] Key)
    {
        string[] bte = new string[16];
        try
        {
            if (bteInput[0] == 0 && bteInput[1] == 0 && bteInput[2] == 0)
            {
                for (int i = 0; i < 16; i++)
                {
                    output[i] = 0;
                }
                output[15] = bteInput[15];
            }
            else
            {
                KeyExpansion(Key);
                for (int i = 0; i < 4 * 4; i++)
                {
                    _state_AES[i % 4, i / 4] = bteInput[i];
                }
                AddRoundKey(_nr_AES);

                for (int round = _nr_AES - 1; round >= 1; round--)  // main round loop
                {
                    InvShiftRows();
                    InvSubBytes();
                    AddRoundKey(round);
                    InvMixColumns();
                }  // end main round loop for InvCipher
                InvShiftRows();
                InvSubBytes();
                AddRoundKey(0);
                // output = state
                for (int i = 0; i < 4 * 4; i++)
                {
                    output[i] = _state_AES[i % 4, i / 4];
                }
            }
            return;
        }
        catch
        {
        }
    }

    private void InvShiftRows()
    {
        byte[,] temp1 = new byte[4, 4];
        for (int r = 0; r < 4; ++r)  // copy State into temp1[]
        {
            for (int c = 0; c < 4; ++c)
            {
                temp1[r, c] = _state_AES[r, c];
            }
        }
        for (int r = 1; r < 4; ++r)  // shift temp1 into State
        {
            for (int c = 0; c < 4; ++c)
            {
                _state_AES[r, (c + r) % _nb_AES] = temp1[r, c];
            }
        }
        return;
    }
    private void InvSubBytes()
    {
        for (int r = 0; r < 4; ++r)
        {
            for (int c = 0; c < 4; ++c)
            {
                _state_AES[r, c] = iSbox[_state_AES[r, c] >> 4, _state_AES[r, c] & 0x0f];
            }
        }
        return;
    }  // InvSubBytes
    private void InvMixColumns()
    {
        byte[,] temp1 = new byte[4, 4];
        for (int r = 0; r < 4; ++r)  // copy State into temp1[]
        {
            for (int c = 0; c < 4; ++c)
            {
                temp1[r, c] = _state_AES[r, c];
            }
        }
        for (int c = 0; c < 4; ++c)
        {
            _state_AES[0, c] = (byte)(gfmultby0e(temp1[0, c]) ^ gfmultby0b(temp1[1, c]) ^ gfmultby0d(temp1[2, c]) ^ gfmultby09(temp1[3, c]));
            _state_AES[1, c] = (byte)(gfmultby09(temp1[0, c]) ^ gfmultby0e(temp1[1, c]) ^ gfmultby0b(temp1[2, c]) ^ gfmultby0d(temp1[3, c]));
            _state_AES[2, c] = (byte)(gfmultby0d(temp1[0, c]) ^ gfmultby09(temp1[1, c]) ^ gfmultby0e(temp1[2, c]) ^ gfmultby0b(temp1[3, c]));
            _state_AES[3, c] = (byte)(gfmultby0b(temp1[0, c]) ^ gfmultby0d(temp1[1, c]) ^ gfmultby09(temp1[2, c]) ^ gfmultby0e(temp1[3, c]));
        }
        return;
    }  // InvMixColumns

    private string DecodData(object obj, IWhichData iWhichData, uint RProg)
    {
        ushort DataCheckSum1 = 0, DataCheckSum2 = 0;
        int DataCheckSumProg1 = 0, DataCheckSumProg2 = 0;
        byte[] bte16A = new byte[16];
        char[] chArr;
        byte[] bte16B = new byte[16];
        byte[] Arrbte = new byte[16];
        byte[] ByteArray = new byte[4];
        byte[] bteData = new byte[16];
        byte[] btebyte, btestr, bte16;
        int SelectFunc1, SelectFunc2, i, a1, a2, a3, a4, a5;
        byte bteCh1, bteCh2;
        ushort bte1, bte2;
        double Result = 0, p1, p2, p3;
        uint aa = 0, RLock = 0;
        string str = "";
        a1 = a2 = a3 = a4 = a5 = 0;
        if (iWhichData == IWhichData.SPECIAL_ID || iWhichData == IWhichData.SERIAL_NUM || iWhichData == IWhichData.PRODUCT_NAME)
        {
            btestr = (byte[])obj;
            try
            {
                if (FuncFirst(obj, RProg))
                {
                    p1 = BitConverter.ToDouble(btestr, 62);
                    p2 = BitConverter.ToDouble(btestr, 70);
                    p3 = BitConverter.ToDouble(btestr, 78);
                    RLock = FuncBig(p1, p2, p3);
                    for (i = 0; i < 16; i++)
                        key_AES[i] = ((byte[])obj)[i + 8];
                }
                SelectFunc1 = ((byte[])obj)[40];
                SelectFunc2 = ((byte[])obj)[59];
                bteCh1 = ((byte[])obj)[41];
                bteCh2 = ((byte[])obj)[42];
                DataCheckSum1 = BytesToWord(bteCh1, bteCh2);
                bteCh1 = ((byte[])obj)[60];
                bteCh2 = ((byte[])obj)[61];
                DataCheckSum2 = BytesToWord(bteCh1, bteCh2);
                for (i = 0; i < 16; i++)
                    bte16A[i] = btestr[i + 24];
                for (i = 0; i < 16; i++)
                    bte16B[i] = btestr[i + 43];
                ReverseAES(bte16A, ref Arrbte, key_AES);
                for (i = 0; i < 16; i++)
                {
                    Arrbte[i] = (byte)(Arrbte[i] ^ RLock);
                    Arrbte[i] = (byte)(Arrbte[i] ^ RProg);
                }
                Result = BitConverter.ToDouble(Arrbte, 0);
                aa = Func64(Result, SelectFunc1);
                ByteArray = BitConverter.GetBytes(aa);
                for (i = 0; i < 4; i++)
                {
                    bteData[3 - i] = ByteArray[i];
                }
                Result = BitConverter.ToDouble(Arrbte, 8);
                aa = Func64(Result, SelectFunc1);
                ByteArray = BitConverter.GetBytes(aa);
                for (i = 0; i < 4; i++)
                    bteData[7 - i] = ByteArray[i];
                DataCheckSumProg1 = 0;
                for (i = 0; i < 8; i++)
                    DataCheckSumProg1 = DataCheckSumProg1 + bteData[i];
                ///////////////////////////////////////////////////////////
                ReverseAES(bte16B, ref Arrbte, key_AES);
                for (i = 0; i < 16; i++)
                {
                    Arrbte[i] = (byte)(Arrbte[i] ^ RLock);
                    Arrbte[i] = (byte)(Arrbte[i] ^ RProg);
                }
                Result = BitConverter.ToDouble(Arrbte, 0);
                aa = Func64(Result, SelectFunc2);
                ByteArray = BitConverter.GetBytes(aa);
                for (i = 0; i < 4; i++)
                {
                    bteData[11 - i] = ByteArray[i];
                }
                Result = BitConverter.ToDouble(Arrbte, 8);
                aa = Func64(Result, SelectFunc2);
                ByteArray = BitConverter.GetBytes(aa);
                for (i = 0; i < 4; i++)
                    bteData[15 - i] = ByteArray[i];
                DataCheckSumProg2 = 0;
                for (i = 0; i < 8; i++)
                    DataCheckSumProg2 = DataCheckSumProg2 + bteData[8 + i];
                if (DataCheckSumProg1 == DataCheckSum1 && DataCheckSumProg2 == DataCheckSum2)
                {
                    if (iWhichData == IWhichData.SPECIAL_ID || iWhichData == IWhichData.PRODUCT_NAME)
                    {
                        chArr = Encoding.Default.GetChars(bteData);
                        for (i = 0; i < 16; i++)
                        {
                            str = str + chArr[i];
                        }
                        str = str.Trim();
                    }
                    else if (iWhichData == IWhichData.SERIAL_NUM)
                    {
                        str = "";
                        byte[] bte6 = new byte[6];
                        for (i = 0; i < 6; i++)
                        {
                            bte6[i] = bteData[i];
                        }
                        str = BitConverter.ToString(bte6).Replace("-", string.Empty);
                        str = str.Insert(4, "-");
                        str = str.Insert(9, "-");
                    }
                }
            }
            catch
            {
            }
        }
        else if (iWhichData == IWhichData.DATA_PARTITION || iWhichData == IWhichData.DESCRIPTION || iWhichData == IWhichData.STRING_VAL1 || iWhichData == IWhichData.STRING_VAL2)
        {

            if (iWhichData == IWhichData.DATA_PARTITION)
            {
                a1 = 531;
                a2 = 48;
                a3 = 24;
                a4 = 240;
                a5 = 30;
            }
            else if (iWhichData == IWhichData.DESCRIPTION || iWhichData == IWhichData.STRING_VAL1 || iWhichData == IWhichData.STRING_VAL2)
            {
                a1 = 307;
                a2 = 40;
                a3 = 16;
                a4 = 128;
                a5 = 16;
            }
            btestr = (byte[])obj;
            try
            {
                if (FuncFirst(obj, RProg))
                {
                    p1 = BitConverter.ToDouble(btestr, a1 - 24);
                    p2 = BitConverter.ToDouble(btestr, a1 - 16);
                    p3 = BitConverter.ToDouble(btestr, a1 - 8);
                    RLock = FuncBig(p1, p2, p3);
                    for (i = 0; i < 16; i++)
                        key_AES[i] = ((byte[])obj)[i + 8];
                }
                btebyte = new byte[a4 * 2];
                SelectFunc1 = ((byte[])obj)[a2];
                bteCh1 = ((byte[])obj)[a2 + 1];
                bteCh2 = ((byte[])obj)[a2 + 2];
                DataCheckSum1 = BytesToWord(bteCh1, bteCh2);
                for (int ii = 0; ii < a3; ii++)
                {
                    btebyte[ii] = btestr[ii + 24];
                }
                for (int ii = a3; ii < a4 * 2; ii++)
                {
                    btebyte[ii] = btestr[ii + 27];
                }
                for (int k = 0; k < a5; k++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        bte16A[j] = btebyte[k * 16 + j];
                    }
                    ReverseAES(bte16A, ref Arrbte, key_AES);
                    for (int h = 0; h < 16; h++)
                    {
                        btestr[16 * k + h] = Arrbte[h];
                    }
                }
                for (i = 0; i < a4 * 2; i++)
                {
                    btestr[i] = (byte)(btestr[i] ^ RLock);
                    btestr[i] = (byte)(btestr[i] ^ RProg);
                }
                DataCheckSumProg1 = 0;
                for (int j = 0; j < a5 * 2; j++)
                {
                    Result = BitConverter.ToDouble(btestr, j * 8);
                    aa = Func64(Result, SelectFunc1);
                    ByteArray = BitConverter.GetBytes(aa);
                    for (i = 0; i < 4; i++)
                    {
                        btestr[j * 4 + i] = ByteArray[3 - i];
                        DataCheckSumProg1 = DataCheckSumProg1 + btestr[j * 4 + i];
                    }
                }
                if (DataCheckSumProg1 == DataCheckSum1)
                {
                    chArr = Encoding.Default.GetChars(btestr);
                    for (i = 0; i < a4; i++)
                    {
                        str = str + chArr[i];
                    }
                    str = str.Trim();
                }
            }
            catch
            {
            }
        }
        else if (iWhichData == IWhichData.PRODUCT_VERSION || iWhichData == IWhichData.NT_USER || iWhichData == IWhichData.INT_VAL1 || iWhichData == IWhichData.INT_VAL2 || iWhichData == IWhichData.INT_VAL3 || iWhichData == IWhichData.INT_VAL4)
        {
            if (iWhichData == IWhichData.PRODUCT_VERSION)
            {
                a1 = 8;
            }
            else if (iWhichData == IWhichData.NT_USER || iWhichData == IWhichData.INT_VAL1 || iWhichData == IWhichData.INT_VAL2 || iWhichData == IWhichData.INT_VAL3 || iWhichData == IWhichData.INT_VAL4)
            {
                a1 = 4;
            }
            btestr = (byte[])obj;
            try
            {
                if (FuncFirst(obj, RProg))
                {
                    p1 = BitConverter.ToDouble(btestr, 43);
                    p2 = BitConverter.ToDouble(btestr, 51);
                    p3 = BitConverter.ToDouble(btestr, 59);
                    RLock = FuncBig(p1, p2, p3);
                    for (i = 0; i < 16; i++)
                        key_AES[i] = ((byte[])obj)[i + 8];
                }
                SelectFunc1 = ((byte[])obj)[40];
                bteCh1 = ((byte[])obj)[41];
                bteCh2 = ((byte[])obj)[42];
                DataCheckSum1 = BytesToWord(bteCh1, bteCh2);
                bte16 = new byte[16];
                for (i = 0; i < 16; i++)
                    bte16[i] = btestr[i + 24];
                ReverseAES(bte16, ref Arrbte, key_AES);
                for (i = 0; i < 16; i++)
                {
                    Arrbte[i] = (byte)(Arrbte[i] ^ RLock);
                    Arrbte[i] = (byte)(Arrbte[i] ^ RProg);
                }
                Result = BitConverter.ToDouble(Arrbte, 0);
                aa = Func64(Result, SelectFunc1);
                ByteArray = BitConverter.GetBytes(aa);
                for (i = 0; i < 4; i++)
                {
                    bteData[3 - i] = ByteArray[i];
                }
                Result = BitConverter.ToDouble(Arrbte, 8);
                aa = Func64(Result, SelectFunc1);
                ByteArray = BitConverter.GetBytes(aa);
                for (i = 0; i < 4; i++)
                    bteData[7 - i] = ByteArray[i];
                DataCheckSumProg1 = 0;
                for (i = 0; i < a1; i++)
                    DataCheckSumProg1 = DataCheckSumProg1 + bteData[i];
                if (DataCheckSumProg1 == DataCheckSum1)
                {
                    if (iWhichData == IWhichData.PRODUCT_VERSION)
                    {
                        chArr = Encoding.Default.GetChars(bteData);
                        for (i = 0; i < 8; i++)
                        {
                            str = str + chArr[i];
                        }
                        str = str.Trim();
                    }
                    else if (iWhichData == IWhichData.NT_USER || iWhichData == IWhichData.INT_VAL1 || iWhichData == IWhichData.INT_VAL2 || iWhichData == IWhichData.INT_VAL3 || iWhichData == IWhichData.INT_VAL4)
                    {
                        if (bteData[0] == 32)
                        {
                            bte1 = bte2 = 0;
                        }
                        else
                        {
                            bte1 = BytesToWord(bteData[0], bteData[1]);
                            bte2 = BytesToWord(bteData[2], bteData[3]);
                        }
                        i = WordsToInt(bte1, bte2);
                        str = i.ToString();
                    }
                }
            }
            catch
            {
            }
        }
        return str;
    }

    private ushort BytesToWord(byte bteH, byte bteL)
    {
        ushort wrdTemp = bteH;
        wrdTemp <<= 8;
        wrdTemp &= 0xff00;
        wrdTemp |= bteL;
        return wrdTemp;
    }

    private int WordsToInt(ushort wrdH, ushort wrdL)
    {
        int iTemp;
        iTemp = wrdH;
        iTemp <<= 16;
        iTemp = (int)(iTemp & 0xffff0000);
        iTemp |= wrdL;
        return iTemp;
    }

    private uint Func64(double x, int a)
    {
        uint Res = 0;
        double R1;
        try
        {
            if (a == 1)
            {
                R1 = 1.0 / 119.0 * Math.Pow(39.0 / 2.0, 0.3333333333333333) * Math.Pow(119 * Math.Sqrt(119) *
            Math.Sqrt(15166431 * x * x - 188184 * x + 260719376) + 5055477 * x -
            31364, 0.3333333333333333) - 14122 * Math.Pow(2, 0.3333333333333333) * Math.Pow(39, 0.6666666666666667)
            /
            (119 * Math.Pow(119 * Math.Sqrt(119) * Math.Sqrt(15166431 * x * x - 188184 * x +
            260719376) + 5055477 * x - 31364, 0.3333333333333333)) - 39.0 / 119.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 2)
            {
                R1 = 1.0 / 177.0 * Math.Pow(47.0 / 2.0, 0.3333333333333333) * Math.Pow(177 * Math.Sqrt(59) *
            Math.Sqrt(16635699 * x * x - 2282196 * x + 114355084) + 5545233 * x -
            380366, 0.3333333333333333) - 10396 * Math.Pow(2, 0.3333333333333333) * Math.Pow(47, 0.6666666666666667)
            /
            (177 * Math.Pow(177 * Math.Sqrt(59) * Math.Sqrt(16635699 * x * x - 2282196 * x +
            114355084) + 5545233 * x - 380366, 0.3333333333333333)) - 47.0 / 177.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 3)
            {
                R1 = Math.Pow(47, 0.3333333333333333) * Math.Pow(111 * Math.Sqrt(111) * Math.Sqrt(643601678076 * x * x - 636636761236 * x + 157440032487) + 938194866 * x - 464020963, 0.3333333333333333) / (777 * Math.Pow(2, 0.6666666666666667))
            - 1804 * Math.Pow(94, 0.6666666666666667) / (777 * Math.Pow(111 * Math.Sqrt(111) * Math.Sqrt(643601678076 * x * x - 636636761236 * x + 157440032487) + 938194866 * x - 464020963, 0.3333333333333333))
            - 47.0 / 111.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 4)
            {
                R1 = Math.Pow(41.0 / 2.0, 0.3333333333333333) * Math.Pow(405 * Math.Sqrt(95) *
            Math.Sqrt(50627136375 * x * x - 32271294648 * x + 5172382576) + 888195375
            * x - 283081532, 0.3333333333333333) / (135 * Math.Pow(19, 0.6666666666666667)) - 5296 *
            Math.Pow(2.0 / 19.0, 0.3333333333333333) * Math.Pow(41, 0.6666666666666667) / (135 * Math.Pow(405 *
            Math.Sqrt(95) * Math.Sqrt(50627136375 * x * x - 32271294648 * x + 5172382576)
            + 888195375 * x - 283081532, 0.3333333333333333)) - 41.0 / 135.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 5)
            {
                R1 = 1.0 / 102.0 * Math.Pow(31, 0.3333333333333333) * Math.Pow(51 * Math.Sqrt(3) * Math.Sqrt(36081072
            * x * x - 16881136 * x + 1995101) + 530604 * x - 124126, 0.3333333333333333) -
            173 * Math.Pow(31, 0.6666666666666667) / (102 * Math.Pow(51 * Math.Sqrt(3) *
            Math.Sqrt(36081072
            * x * x - 16881136 * x + 1995101) + 530604 * x - 124126, 0.3333333333333333))
            -
            31.0 / 102.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 6)
            {
                R1 = Math.Pow(17.0 / 5.0, 0.3333333333333333) * Math.Pow(228 * Math.Sqrt(1311) *
            Math.Sqrt(225324323100 * x * x - 87295810115 * x + 8536314174) +
            3918683880
            * x - 759094001, 0.3333333333333333) / (114 * Math.Pow(23, 0.6666666666666667)) - 8273 *
            Math.Pow(5.0 / 23.0, 0.3333333333333333) * Math.Pow(17, 0.6666666666666667) / (114 * Math.Pow(228 *
            Math.Sqrt(1311) * Math.Sqrt(225324323100 * x * x - 87295810115 * x +
            8536314174)
            + 3918683880 * x - 759094001, 0.3333333333333333)) - 17.0 / 114.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 7)
            {
                R1 = Math.Pow(157.0 / 5.0, 0.3333333333333333) * Math.Pow(228 * Math.Sqrt(1311) *
            Math.Sqrt(225324323100 * x * x - 70643419115 * x + 5707910974) +
            3918683880
            * x - 614290601, 0.3333333333333333) / (114 * Math.Pow(23, 0.6666666666666667)) - 5053 *
            Math.Pow(5.0 / 23.0, 0.3333333333333333) * Math.Pow(157, 0.6666666666666667) / (114 * Math.Pow(228
            *
            Math.Sqrt(1311) * Math.Sqrt(225324323100 * x * x - 70643419115 * x +
            5707910974)
            + 3918683880 * x - 614290601, 0.3333333333333333)) - 157.0 / 114.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 8)
            {
                R1 = Math.Pow(217.0 / 10.0, 0.3333333333333333) * Math.Pow(891 * Math.Sqrt(1221) *
            Math.Sqrt(33175274791725 * x * x - 12198404215540 * x + 1129413164564) +
            179325809685 * x - 32968660042, 0.3333333333333333) / (297 * Math.Pow(37, 0.6666666666666667))
            - 21374 * Math.Pow(10.0 / 37.0, 0.3333333333333333) * Math.Pow(217, 0.6666666666666667) / (297 *
            Math.Pow(891 * Math.Sqrt(1221) * Math.Sqrt(33175274791725 * x * x -
            12198404215540 * x + 1129413164564) + 179325809685 * x - 32968660042, 0.3333333333333333)) -
            217.0 / 297.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 9)
            {
                R1 = Math.Pow(83.0 / 10.0, 0.3333333333333333) * Math.Pow(43 * Math.Sqrt(1419) *
            Math.Sqrt(52073254750275 * x * x - 20820882351060 * x + 2081173026476) +
            11688721605 * x - 2336799366, 0.3333333333333333) / (129 * Math.Pow(33, 0.6666666666666667))
            + 890 * Math.Pow(10.0 / 33.0, 0.3333333333333333) * Math.Pow(83, 0.6666666666666667) / (129 *
            Math.Pow(43 * Math.Sqrt(1419) * Math.Sqrt(52073254750275 * x * x - 20820882351060
            * x + 2081173026476) + 11688721605 * x - 2336799366, 0.3333333333333333)) - 83.0 / 129.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 10)
            {
                R1 = Math.Pow(23.0 / 10.0, 0.3333333333333333) * Math.Pow(97 * Math.Sqrt(1067) *
            Math.Sqrt(2459904720075 * x * x - 895021243380 * x + 82326208748) +
            4969504485 * x - 904061862, 0.3333333333333333) / (97 * Math.Pow(11, 0.6666666666666667)) -
            7132 * Math.Pow(10.0 / 11.0, 0.3333333333333333) * Math.Pow(23, 0.6666666666666667) / (97 *
            Math.Pow(97 * Math.Sqrt(1067) * Math.Sqrt(2459904720075 * x * x - 895021243380 *
            x + 82326208748) + 4969504485 * x - 904061862, 0.3333333333333333)) - 69.0 / 97.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 11)
            {
                R1 = 1.0 / 159.0 * (Math.Pow(103.0 / 2.0, 0.3333333333333333) * Math.Pow(159 * Math.Sqrt(159) *
            Math.Sqrt(4019679 * x * x - 42436 * x) + 4019679 * x - 21218, 0.3333333333333333) +
            103 * Math.Pow(2, 0.3333333333333333) * Math.Pow(103, 0.6666666666666667) / Math.Pow(159 *
            Math.Sqrt(159) * Math.Sqrt(4019679 * x * x - 42436 * x) + 4019679 * x -
            21218,
            0.3333333333333333) - 103);
                Res = (uint)Math.Round(R1);
            }
            else if (a == 12)
            {
                R1 = 1.0 / 87.0 * Math.Pow(61.0 / 362.0, 0.3333333333333333) * Math.Pow(87 * Math.Sqrt(29) *
            Math.Sqrt(64719650349 * x * x - 1701222258 * x + 2913505) + 119189043 * x
            -
            1566503, 0.3333333333333333) + 61 * Math.Pow(61, 0.6666666666666667) * Math.Pow(362, 0.3333333333333333)
            /
            (87 * Math.Pow(87 * Math.Sqrt(29) * Math.Sqrt(64719650349 * x * x - 1701222258 *
            x
            + 2913505) + 119189043 * x - 1566503, 0.3333333333333333)) - 61.0 / 87.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 13)
            {
                R1 = 1.0 / 93.0 * Math.Pow(79.0 / 886.0, 0.3333333333333333) * Math.Pow(93 * Math.Sqrt(93) *
            Math.Sqrt(157854256893 * x * x - 5611820338 * x + 11863409) + 356330151 *
            x - 6333883, 0.3333333333333333) + 79 * Math.Pow(79, 0.6666666666666667) * Math.Pow(886, 0.3333333333333333)
            / (93 * Math.Pow(93 * Math.Sqrt(93) * Math.Sqrt(157854256893 * x * x - 5611820338 * x + 11863409) + 356330151 * x - 6333883, 0.3333333333333333))
            - 79.0 / 93.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 14)
            {
                R1 = 1.0 / 129.0 * Math.Pow(83.0 / 886.0, 0.3333333333333333) * Math.Pow(129 * Math.Sqrt(129) *
            Math.Sqrt(421285569561 * x * x - 7309803898 * x + 14353997) + 950983227 * x - 8250343, 0.3333333333333333)
            + 83 * Math.Pow(83, 0.6666666666666667) * Math.Pow(886, 0.3333333333333333)
            / (129 * Math.Pow(129 * Math.Sqrt(129) * Math.Sqrt(421285569561 * x * x -
            7309803898 * x + 14353997) + 950983227 * x - 8250343, 0.3333333333333333)) - 83.0 / 129.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 15)
            {
                R1 = Math.Pow(97, 0.3333333333333333) * Math.Pow(219 * Math.Sqrt(23871) *
            Math.Sqrt(54409136021244 * x * x - 54161910945604 * x + 13478975752223) +
            249583192758 * x - 124224566389, 0.3333333333333333) / (219 * Math.Pow(218, 0.6666666666666667)) - 5414 * Math.Pow(194, 0.6666666666666667) / (219 * Math.Pow(109, 0.3333333333333333) *
            Math.Pow(219 * Math.Sqrt(23871) * Math.Sqrt(54409136021244 * x * x -
            54161910945604
            * x + 13478975752223) + 249583192758 * x - 124224566389, 0.3333333333333333))
            - 97.0 / 219.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 16)
            {
                R1 = Math.Pow(91.0 / 5.0, 0.3333333333333333) * Math.Pow(152 * Math.Sqrt(437) *
            Math.Sqrt(300432430800 * x * x - 96448869645 * x + 7999737322) +
            1741637280
            * x - 279561941, 0.3333333333333333) / (76 * Math.Pow(23, 0.6666666666666667)) - 3683 *
            Math.Pow(5.0 / 23.0, 0.3333333333333333) * Math.Pow(91, 0.6666666666666667) / (76 * Math.Pow(152 *
            Math.Sqrt(437) * Math.Sqrt(300432430800 * x * x - 96448869645 * x + 7999737322)
            + 1741637280 * x - 279561941, 0.3333333333333333)) - 91.0 / 76.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 17)
            {
                R1 = Math.Pow(41.0 / 5.0, 0.3333333333333333) * Math.Pow(704 * Math.Sqrt(1991) *
            Math.Sqrt(1818428606438400 * x * x - 723438247827135 * x + 71955089316991)
            + 1339542251520 * x - 266459759789, 0.3333333333333333) / (176 * Math.Pow(181, 0.6666666666666667)) - 23555 * Math.Pow(5.0 / 181.0, 0.3333333333333333) * Math.Pow(41, 0.6666666666666667) /
            (176 * Math.Pow(704 * Math.Sqrt(1991) * Math.Sqrt(1818428606438400 * x * x -
            723438247827135 * x + 71955089316991) + 1339542251520 * x -
            266459759789, 0.3333333333333333)) - 41.0 / 176.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 18)
            {
                R1 = Math.Pow(31.0 / 5.0, 0.3333333333333333) * Math.Pow(704 * Math.Sqrt(1991) *
            Math.Sqrt(1818428606438400 * x * x - 724259697141135 * x + 72118446291791)
            + 1339542251520 * x - 266762319389, 0.3333333333333333) / (176 * Math.Pow(181, 0.6666666666666667)) - 25365 * Math.Pow(5.0 / 181.0, 0.3333333333333333) * Math.Pow(31, 0.6666666666666667) /
            (176 * Math.Pow(704 * Math.Sqrt(1991) * Math.Sqrt(1818428606438400 * x * x -
            724259697141135 * x + 72118446291791) + 1339542251520 * x -
            266762319389, 0.3333333333333333)) - 31.0 / 176.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 19)
            {
                R1 = Math.Pow(46.0 / 5.0, 0.3333333333333333) * Math.Pow(783 * Math.Sqrt(3683) *
            Math.Sqrt(910484786688075 * x * x - 360873537445030 * x + 35759036280683)
            + 1433834309745 * x - 284152391689, 0.3333333333333333) / (261 * Math.Pow(127, 0.6666666666666667)) - 22046 * Math.Pow(5.0 / 127.0, 0.3333333333333333) * Math.Pow(46, 0.6666666666666667) /
            (261 * Math.Pow(783 * Math.Sqrt(3683) * Math.Sqrt(910484786688075 * x * x -
            360873537445030 * x + 35759036280683) + 1433834309745 * x -
            284152391689, 0.3333333333333333)) - 92.0 / 261.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 20)
            {
                R1 = Math.Pow(67.0 / 5.0, 0.3333333333333333) * Math.Pow(208 * Math.Sqrt(806) *
            Math.Sqrt(1884983817600 * x * x - 685677310365 * x + 63348896519) +
            8107457280 * x - 1474574861, 0.3333333333333333) / (104 * Math.Pow(31, 0.6666666666666667))
            - 8739 * Math.Pow(5.0 / 31.0, 0.3333333333333333) * Math.Pow(67, 0.6666666666666667) / (104 *
            Math.Pow(208 * Math.Sqrt(806) * Math.Sqrt(1884983817600 * x * x - 685677310365 *
            x + 63348896519) + 8107457280 * x - 1474574861, 0.3333333333333333)) - 67.0 / 104.0
            ;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 21)
            {
                R1 = Math.Pow(211, 0.3333333333333333) * Math.Pow(567 * Math.Sqrt(483) * Math.Sqrt(328570759692
            * x * x - 305340613412 * x + 71113731739) + 7142842602 * x -
            3318919711, 0.3333333333333333) / (189 * Math.Pow(46, 0.6666666666666667)) - 7054 *
            Math.Pow(422, 0.6666666666666667) / (189 * Math.Pow(23, 0.3333333333333333) * Math.Pow(567 *
            Math.Sqrt(483) * Math.Sqrt(328570759692 * x * x - 305340613412 * x +
            71113731739)
            + 7142842602 * x - 3318919711, 0.3333333333333333)) - 211.0 / 189.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 22)
            {
                R1 = Math.Pow(83, 0.3333333333333333) * Math.Pow(141 * Math.Sqrt(9165) * Math.Sqrt(3079338268500
            * x * x - 3053834377100 * x + 757136314789) + 23687217450 * x -
            11745516835, 0.3333333333333333) / (141 * Math.Pow(130, 0.6666666666666667)) - 1232 *
            Math.Pow(166, 0.6666666666666667) / (141 * Math.Pow(65, 0.3333333333333333) * Math.Pow(141 *
            Math.Sqrt(9165) * Math.Sqrt(3079338268500 * x * x - 3053834377100 * x +
            757136314789) + 23687217450 * x - 11745516835, 0.3333333333333333)) - 83.0 / 141.0
            ;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 23)
            {
                R1 = Math.Pow(167, 0.3333333333333333) * Math.Pow(402 * Math.Sqrt(38793) *
            Math.Sqrt(233517853853028 * x * x - 232913573979242 * x + 58077766532441)
            + 1209937066596 * x - 603403041397, 0.3333333333333333) / (402 * Math.Pow(193, 0.6666666666666667)) - 21637 * Math.Pow(167, 0.6666666666666667) / (402 * Math.Pow(193, 0.3333333333333333) *
            Math.Pow(402 * Math.Sqrt(38793) * Math.Sqrt(233517853853028 * x * x -
            232913573979242 * x + 58077766532441) + 1209937066596 * x -
            603403041397, 0.3333333333333333)) - 167.0 / 402.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 24)
            {
                R1 = Math.Pow(79, 0.3333333333333333) * Math.Pow(111 * Math.Sqrt(10767) *
            Math.Sqrt(4992799550652 * x * x - 5010668747092 * x + 1257108915479) +
            25736080158 * x - 12914094709, 0.3333333333333333) / (111 * Math.Pow(194, 0.6666666666666667))
            + 3556 * Math.Pow(158, 0.6666666666666667) / (111 * Math.Pow(97, 0.3333333333333333) *
            Math.Pow(111 * Math.Sqrt(10767) * Math.Sqrt(4992799550652 * x * x - 5010668747092 * x +
            1257108915479) + 25736080158 * x - 12914094709, 0.3333333333333333)) - 79.0 / 111.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 25)
            {
                R1 = Math.Pow(67, 0.3333333333333333) * Math.Pow(34 * Math.Sqrt(527) * Math.Sqrt(426795041628 *
            x * x - 285722603766 * x + 47817366257) + 509910444 * x -
            170682559, 0.3333333333333333) / (102 * Math.Pow(31, 0.6666666666666667)) + 307 * Math.Pow(67, 0.6666666666666667)
            / (34 * Math.Pow(31, 0.3333333333333333) * Math.Pow(34 * Math.Sqrt(527) * Math.Sqrt(426795041628
            * x * x - 285722603766 * x + 47817366257) + 509910444 * x -
            170682559,
            0.3333333333333333)) - 67.0 / 102.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 26)
            {
                R1 = Math.Pow(107, 0.3333333333333333) * Math.Pow(124 * Math.Sqrt(2542) *
            Math.Sqrt(47897644720608 * x * x - 47716238418822 * x + 11884028736721) +
            43267971744 * x - 21552049873, 0.3333333333333333) / (372 * Math.Pow(41, 0.6666666666666667))
            - 3663 * Math.Pow(107, 0.6666666666666667) / (124 * Math.Pow(41, 0.3333333333333333) *
            Math.Pow(124
            * Math.Sqrt(2542) * Math.Sqrt(47897644720608 * x * x - 47716238418822 * x +
            11884028736721) + 43267971744 * x - 21552049873, 0.3333333333333333)) - 107.0 / 372.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 27)
            {
                R1 = Math.Pow(3, 0.3333333333333333) * Math.Pow(16 * Math.Sqrt(6357) * Math.Sqrt(43238178048 * x
            * x - 43117182822 * x + 10749143671) + 265264896 * x - 132261297, 0.3333333333333333) / (8 * Math.Pow(163, 0.6666666666666667)) - 343 * Math.Pow(3, 0.6666666666666667) / (8
            * Math.Pow(163, 0.3333333333333333) * Math.Pow(16 * Math.Sqrt(6357) * Math.Sqrt(43238178048 * x
            * x - 43117182822 * x + 10749143671) + 265264896 * x - 132261297, 0.3333333333333333)) - 3.0 / 8.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 28)
            {
                R1 = Math.Pow(29, 0.3333333333333333) * Math.Pow(114 * Math.Sqrt(2337) * Math.Sqrt(459492723108
            * x * x - 304775631426 * x + 50539657567) + 3735713196 * x -
            1238925331, 0.3333333333333333) / (114 * Math.Pow(41, 0.6666666666666667)) - 3143 *
            Math.Pow(29, 0.6666666666666667) / (114 * Math.Pow(41, 0.3333333333333333) * Math.Pow(114 *
            Math.Sqrt(2337) * Math.Sqrt(459492723108 * x * x - 304775631426 * x +
            50539657567) + 3735713196 * x - 1238925331, 0.3333333333333333)) - 29.0 / 114.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 29)
            {
                R1 = Math.Pow(43.0 / 2.0, 0.3333333333333333) * Math.Pow(111 * Math.Sqrt(15873) *
            Math.Sqrt(35993125292553 * x * x - 23865288218772 * x + 3956007114308) +
            83900058957 * x - 27815021234, 0.3333333333333333) / (111 * Math.Pow(143, 0.6666666666666667))
            - 6172 * Math.Pow(2.0 / 143.0, 0.3333333333333333) * Math.Pow(43, 0.6666666666666667) / (111 *
            Math.Pow(111 * Math.Sqrt(15873) * Math.Sqrt(35993125292553 * x * x -
            23865288218772 * x + 3956007114308) + 83900058957 * x - 27815021234, 0.3333333333333333)) - 43.0 / 111.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 30)
            {
                R1 = 1.0 / 861.0 * Math.Pow(2.0 / 103.0, 0.6666666666666667) * Math.Pow(17, 0.3333333333333333) *
            Math.Pow(861 * Math.Sqrt(29561) * Math.Sqrt(2092388783123961 * x * x -
            1393325251397454 * x + 231992585700637) + 6771484735029 * x -
            2254571604203, 0.3333333333333333) - 984926 * Math.Pow(2.0 / 103.0, 0.3333333333333333) *
            Math.Pow(17, 0.6666666666666667) / (861 * Math.Pow(861 * Math.Sqrt(29561) *
            Math.Sqrt(2092388783123961 * x * x - 1393325251397454 * x +
            231992585700637) + 6771484735029 * x - 2254571604203, 0.3333333333333333)) - 34.0 / 861.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 31)
            {
                R1 = Math.Pow(143, 0.3333333333333333) * Math.Pow(414 * Math.Sqrt(6693) *
            Math.Sqrt(97142099436468 * x * x - 64181545855818 * x + 10602126304859) +
            333821647548 * x - 110277570199, 0.3333333333333333) / (414 * Math.Pow(97, 0.6666666666666667)) - 43261 * Math.Pow(143, 0.6666666666666667) / (414 * Math.Pow(97, 0.3333333333333333) *
            Math.Pow(414 * Math.Sqrt(6693) * Math.Sqrt(97142099436468 * x * x -
            64181545855818
            * x + 10602126304859) + 333821647548 * x - 110277570199, 0.3333333333333333))
            - 143.0 / 414.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 32)
            {
                R1 = Math.Pow(7.0 / 67.0, 0.6666666666666667) * Math.Pow(143 * Math.Sqrt(9581) *
            Math.Sqrt(7915439429469 * x * x - 5204661518400 * x + 855898035488) +
            39380295669 * x - 12946919200, 0.3333333333333333) / (143 * Math.Pow(2, 0.3333333333333333))
            - 120162 * Math.Pow(14.0 / 67.0, 0.3333333333333333) / (143 * Math.Pow(143 *
            Math.Sqrt(9581)
            * Math.Sqrt(7915439429469 * x * x - 5204661518400 * x + 855898035488) +
            39380295669 * x - 12946919200, 0.3333333333333333)) - 49.0 / 143.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 33)
            {
                R1 = Math.Pow(167.0 / 2.0, 0.3333333333333333) * Math.Pow(309 * Math.Sqrt(4841) *
            Math.Sqrt(9189465821001 * x * x - 5949717419040 * x + 963972174752) +
            65173516461 * x - 21098288720, 0.3333333333333333) / (309 * Math.Pow(47, 0.6666666666666667))
            - 23978 * Math.Pow(2.0 / 47.0, 0.3333333333333333) * Math.Pow(167, 0.6666666666666667) / (309 *
            Math.Pow(309 * Math.Sqrt(4841) * Math.Sqrt(9189465821001 * x * x - 5949717419040
            *
            x + 963972174752) + 65173516461 * x - 21098288720, 0.3333333333333333)) - 167.0 / 309.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 34)
            {
                R1 = Math.Pow(59.0 / 2.0, 0.3333333333333333) * Math.Pow(133 * Math.Sqrt(21) *
            Math.Sqrt(10862125029 * x * x - 7093774260 * x + 1159007348) + 63521199 *
            x - 20742030, 0.3333333333333333) / (133 * Math.Pow(3, 0.6666666666666667)) - 754 * Math.Pow(2.0 / 3.0, 0.3333333333333333)
            * Math.Pow(59, 0.6666666666666667) / (133 * Math.Pow(133 * Math.Sqrt(21) *
            Math.Sqrt(10862125029 * x * x - 7093774260 * x + 1159007348) + 63521199 *
            x - 20742030, 0.3333333333333333)) - 59.0 / 133.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 35)
            {
                R1 = Math.Pow(197.0 / 10.0, 0.3333333333333333) * Math.Pow(1377 * Math.Sqrt(2397) *
            Math.Sqrt(250998796487925 * x * x - 96218311135220 * x + 9267290546452) +
            1068079985055 * x - 204719810926, 0.3333333333333333) / (459 * Math.Pow(47, 0.6666666666666667)) - 60968 * Math.Pow(10.0 / 47.0, 0.3333333333333333) * Math.Pow(197, 0.6666666666666667) /
            (459
            * Math.Pow(1377 * Math.Sqrt(2397) * Math.Sqrt(250998796487925 * x * x -
            96218311135220 * x + 9267290546452) + 1068079985055 * x -
            204719810926, 0.3333333333333333)) - 197.0 / 459.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 36)
            {
                R1 = Math.Pow(19, 0.3333333333333333) * Math.Pow(1431 * Math.Sqrt(53) *
            Math.Sqrt(319215069902925 * x * x - 122920918125670 * x + 11884923352517)
            +
            186131236095 * x - 35837002369, 0.3333333333333333) / 3339.0 - 665330 *
            Math.Pow(19, 0.6666666666666667) / (3339 * Math.Pow(1431 * Math.Sqrt(53) *
            Math.Sqrt(319215069902925 * x * x - 122920918125670 * x + 11884923352517)
            +
            186131236095 * x - 35837002369, 0.3333333333333333)) - 190.0 / 477.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 37)
            {
                R1 = 1.0 / 507.0 * Math.Pow(209.0 / 10.0, 0.3333333333333333) * Math.Pow(2197 *
            Math.Sqrt(87968594025 * x * x - 33445725660 * x + 3213042596) + 651619215
            *
            x - 123873058, 0.3333333333333333) - 1988 * Math.Pow(10, 0.3333333333333333) * Math.Pow(209, 0.6666666666666667) / (507 * Math.Pow(2197 * Math.Sqrt(87968594025 * x * x - 33445725660
            *
            x + 3213042596) + 651619215 * x - 123873058, 0.3333333333333333)) - 209.0 / 507.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 38)
            {
                R1 = Math.Pow(73.0 / 10.0, 0.3333333333333333) * Math.Pow(179 * Math.Sqrt(5191) *
            Math.Sqrt(31472816145975 * x * x - 11742925251840 * x + 1129465666304) +
            72351301485 * x - 13497615232, 0.3333333333333333) / (179 * Math.Pow(29, 0.6666666666666667))
            - 29924 * Math.Pow(10.0 / 29.0, 0.3333333333333333) * Math.Pow(73, 0.6666666666666667) / (179 *
            Math.Pow(179 * Math.Sqrt(5191) * Math.Sqrt(31472816145975 * x * x -
            11742925251840
            * x + 1129465666304) + 72351301485 * x - 13497615232, 0.3333333333333333)) - 73.0 / 179.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 39)
            {
                R1 = 1.0 / 27.0 * Math.Pow(23.0 / 2.0, 0.3333333333333333) * Math.Pow(9 * Math.Sqrt(3) *
            Math.Sqrt(78121827 * x * x - 18590796 * x + 1130252) + 137781 * x -
            16394, 0.3333333333333333) - 40 * Math.Pow(2, 0.3333333333333333) * Math.Pow(23, 0.6666666666666667) /
            (27 * Math.Pow(9 * Math.Sqrt(3) * Math.Sqrt(78121827 * x * x - 18590796 * x +
            1130252) + 137781 * x - 16394, 0.3333333333333333)) - 23.0 / 27.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 40)
            {
                R1 = 1.0 / 567.0 * Math.Pow(239.0 / 10.0, 0.3333333333333333) * Math.Pow(729 *
            Math.Sqrt(1563087555225 * x * x - 600301717540 * x + 57968708324) +
            911421315 * x - 175015078, 0.3333333333333333) - 1948 * Math.Pow(10, 0.3333333333333333) *
            Math.Pow(239, 0.6666666666666667) / (567 * Math.Pow(729 * Math.Sqrt(1563087555225 * x * x
            - 600301717540 * x + 57968708324) + 911421315 * x - 175015078, 0.3333333333333333)) - 239.0 / 567.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 41)
            {
                R1 = Math.Pow(227.0 / 5.0, 0.3333333333333333) * Math.Pow(918 * Math.Sqrt(629) *
            Math.Sqrt(18141761978100 * x * x - 6659744606390 * x + 615892272379) +
            98063578260 * x - 17999309747, 0.3333333333333333) / (306 * Math.Pow(37, 0.6666666666666667))
            - 22813 * Math.Pow(5.0 / 37.0, 0.3333333333333333) * Math.Pow(227, 0.6666666666666667) / (306 *
            Math.Pow(918 * Math.Sqrt(629) * Math.Sqrt(18141761978100 * x * x - 6659744606390
            *
            x + 615892272379) + 98063578260 * x - 17999309747, 0.3333333333333333)) - 227.0 / 306.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 42)
            {
                R1 = Math.Pow(263.0 / 2.0, 0.3333333333333333) * Math.Pow(143 * Math.Sqrt(559) *
            Math.Sqrt(1864376811185031 * x * x - 327790556955180 * x + 14410872079100)
            + 145985186061 * x - 12833394290, 0.3333333333333333) / (429 * Math.Pow(43, 0.6666666666666667)) - 9140 * Math.Pow(2.0 / 43.0, 0.3333333333333333) * Math.Pow(263, 0.6666666666666667) / (429
            * Math.Pow(143 * Math.Sqrt(559) * Math.Sqrt(1864376811185031 * x * x -
            327790556955180 * x + 14410872079100) + 145985186061 * x -
            12833394290, 0.3333333333333333)) - 263.0 / 429.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 43)
            {
                R1 = Math.Pow(257.0 / 5.0, 0.3333333333333333) * Math.Pow(1344 * Math.Sqrt(777) *
            Math.Sqrt(12008894284800 * x * x - 4390690484245 * x + 405098318757) +
            129825884160 * x - 23733462077, 0.3333333333333333) / (336 * Math.Pow(37, 0.6666666666666667))
            - 28123 * Math.Pow(5.0 / 37.0, 0.3333333333333333) * Math.Pow(257, 0.6666666666666667) / (336 *
            Math.Pow(1344 * Math.Sqrt(777) * Math.Sqrt(12008894284800 * x * x - 4390690484245
            * x + 405098318757) + 129825884160 * x - 23733462077, 0.3333333333333333)) -
            257.0 / 336.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 44)
            {
                R1 = Math.Pow(239.0 / 5.0, 0.3333333333333333) * Math.Pow(366 * Math.Sqrt(8601) *
            Math.Sqrt(63627790580100 * x * x - 23979584807290 * x + 2268410574179) +
            270756555660 * x - 51020393207, 0.3333333333333333) / (366 * Math.Pow(47, 0.6666666666666667))
            - 33419 * Math.Pow(5.0 / 47.0, 0.3333333333333333) * Math.Pow(239, 0.6666666666666667) / (366 *
            Math.Pow(366 * Math.Sqrt(8601) * Math.Sqrt(63627790580100 * x * x -
            23979584807290
            * x + 2268410574179) + 270756555660 * x - 51020393207, 0.3333333333333333)) -
            239.0 / 366.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 45)
            {
                R1 = Math.Pow(89.0 / 7.0, 0.3333333333333333) * Math.Pow(184 * Math.Sqrt(851) *
            Math.Sqrt(4348577879856 * x * x - 1074037197135 * x + 67092362992) +
            11193250656 * x - 1382287255, 0.3333333333333333) / (92 * Math.Pow(37, 0.6666666666666667))
            - 5171 * Math.Pow(7.0 / 37.0, 0.3333333333333333) * Math.Pow(89, 0.6666666666666667) / (92 *
            Math.Pow(184 * Math.Sqrt(851) * Math.Sqrt(4348577879856 * x * x - 1074037197135 * x
            + 67092362992) + 11193250656 * x - 1382287255, 0.3333333333333333)) - 89.0 / 92.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 46)
            {
                R1 = 1.0 / 216.0 * Math.Pow(277.0 / 7.0, 0.3333333333333333) * Math.Pow(144 * Math.Sqrt(6) *
            Math.Sqrt(9999593856 * x * x - 2211671385 * x + 125208397) + 35271936 * x -
            3900655, 0.3333333333333333) - 299 * Math.Pow(7, 0.3333333333333333) * Math.Pow(277, 0.6666666666666667) /
            (216 * Math.Pow(144 * Math.Sqrt(6) * Math.Sqrt(9999593856 * x * x - 2211671385 * x
            + 125208397) + 35271936 * x - 3900655, 0.3333333333333333)) - 277.0 / 216.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 47)
            {
                R1 = Math.Pow(73.0 / 7.0, 0.3333333333333333) * Math.Pow(152 * Math.Sqrt(551) *
            Math.Sqrt(1180356969456 * x * x - 278159672679 * x + 16786762492) +
            3876377568 * x - 456748231, 0.3333333333333333) / (76 * Math.Pow(29, 0.6666666666666667)) -
            3659 * Math.Pow(7.0 / 29.0, 0.3333333333333333) * Math.Pow(73, 0.6666666666666667) / (76 *
            Math.Pow(152 * Math.Sqrt(551) * Math.Sqrt(1180356969456 * x * x - 278159672679 * x
            + 16786762492) + 3876377568 * x - 456748231, 0.3333333333333333)) - 73.0 / 76.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 48)
            {
                R1 = Math.Pow(119, 0.3333333333333333) * Math.Pow(704 * Math.Sqrt(473) *
            Math.Sqrt(78997056095232 * x * x - 17309265567057 * x + 948547815709) +
            136084506624 * x - 14908928137, 0.3333333333333333) / (528 * Math.Pow(43, 0.6666666666666667))
            - 25859 * Math.Pow(119, 0.6666666666666667) / (528 * Math.Pow(43, 0.3333333333333333) *
            Math.Pow(704 * Math.Sqrt(473) * Math.Sqrt(78997056095232 * x * x - 17309265567057
            * x + 948547815709) + 136084506624 * x - 14908928137, 0.3333333333333333)) - 119.0 / 528.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 49)
            {
                R1 = Math.Pow(109, 0.3333333333333333) * Math.Pow(106 * Math.Sqrt(18921) * Math.Sqrt(243856736782596 * x * x - 53145962043426 * x + 2896192268587) +
            227690697276 * x - 24811373503, 0.3333333333333333) / (318 * Math.Pow(119, 0.6666666666666667))
            - 20737 * Math.Pow(109, 0.6666666666666667) / (318 * Math.Pow(119, 0.3333333333333333) * Math.Pow(106 * Math.Sqrt(18921) * Math.Sqrt(243856736782596 * x * x -
            53145962043426 * x + 2896192268587) + 227690697276 * x - 24811373503, 0.3333333333333333))
            - 109.0 / 318.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 50)
            {
                R1 = Math.Pow(113, 0.3333333333333333) * Math.Pow(378 * Math.Sqrt(903) *
            Math.Sqrt(19323833197788 * x * x - 4222897940034 * x + 230761144049) +
            49932385524 * x - 5455940491, 0.3333333333333333) / (378 * Math.Pow(43, 0.6666666666666667))
            - 11017 * Math.Pow(113, 0.6666666666666667) / (378 * Math.Pow(43, 0.3333333333333333) *
            Math.Pow(378 * Math.Sqrt(903) * Math.Sqrt(19323833197788 * x * x - 4222897940034
            * x + 230761144049) + 49932385524 * x - 5455940491, 0.3333333333333333)) - 113.0 / 378.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 51)
            {
                R1 = Math.Pow(16 * Math.Sqrt(3706) * Math.Sqrt(228256631424 * x * x - 49467814767
            * x + 2681535337) + 465355008 * x - 50425907, 0.3333333333333333) / (8 *
            Math.Pow(109, 0.6666666666666667)) - 2283.0 / (8 * Math.Pow(109, 0.3333333333333333) * Math.Pow(16 *
            Math.Sqrt(3706) * Math.Sqrt(228256631424 * x * x - 49467814767 * x +
            2681535337) + 465355008 * x - 50425907, 0.3333333333333333)) - 3.0 / 8.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 52)
            {
                R1 = Math.Pow(163.0 / 2.0, 0.3333333333333333) * Math.Pow(369 * Math.Sqrt(4879) *
            Math.Sqrt(762013195803279 * x * x - 165290825202804 * x + 8965496479676)
            + 711496914849 * x - 77166585062, 0.3333333333333333) / (369 * Math.Pow(119, 0.6666666666666667)) - 25990 * Math.Pow(2.0 / 119.0, 0.3333333333333333) * Math.Pow(163, 0.6666666666666667) /
            (369 * Math.Pow(369 * Math.Sqrt(4879) * Math.Sqrt(762013195803279 * x * x -
            165290825202804 * x + 8965496479676) + 711496914849 * x -
            77166585062, 0.3333333333333333)) - 163.0 / 369.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 53)
            {
                R1 = Math.Pow(19.0 / 2.0, 0.3333333333333333) * Math.Pow(489 * Math.Sqrt(67971) *
            Math.Sqrt(37997615908292931 * x * x - 6731762496360220 * x +
            298266854933484) + 24851285747739 * x - 2201361182590, 0.3333333333333333) /
            (489 * Math.Pow(139, 0.6666666666666667)) - 557216 * Math.Pow(2.0 / 139.0, 0.3333333333333333) *
            Math.Pow(19, 0.6666666666666667) / (489 * Math.Pow(489 * Math.Sqrt(67971) *
            Math.Sqrt(37997615908292931 * x * x - 6731762496360220 * x +
            298266854933484) + 24851285747739 * x - 2201361182590, 0.3333333333333333)) -
            209.0 / 489.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 54)
            {
                R1 = Math.Pow(215.0 / 22.0, 0.3333333333333333) * Math.Pow(489 * Math.Sqrt(67971) *
            Math.Sqrt(37997615908292931 * x * x - 6728360965759312 * x +
            297963226999776) + 24851285747739 * x - 2200248844264, 0.3333333333333333) /
            (489 * Math.Pow(139, 0.6666666666666667)) - 49822 * Math.Pow(22.0 / 139.0, 0.3333333333333333) *
            Math.Pow(215, 0.6666666666666667) / (489 * Math.Pow(489 * Math.Sqrt(67971) *
            Math.Sqrt(37997615908292931 * x * x - 6728360965759312 * x +
            297963226999776) + 24851285747739 * x - 2200248844264, 0.3333333333333333)) -
            215.0 / 489.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 55)
            {
                R1 = Math.Pow(107.0 / 11.0, 0.3333333333333333) * Math.Pow(173 * Math.Sqrt(8131) *
            Math.Sqrt(47418169181435019 * x * x - 8406286100299494 * x +
            372723249480419) + 3396960325341 * x - 301106314933, 0.3333333333333333) /
            (519
            * Math.Pow(47, 0.6666666666666667)) - 39742 * Math.Pow(11.0 / 47.0, 0.3333333333333333) *
            Math.Pow(107, 0.6666666666666667) / (519 * Math.Pow(173 * Math.Sqrt(8131) *
            Math.Sqrt(47418169181435019 * x * x - 8406286100299494 * x +
            372723249480419) + 3396960325341 * x - 301106314933, 0.3333333333333333)) - 214.0 / 519.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 56)
            {
                R1 = Math.Pow(97, 0.3333333333333333) * Math.Pow(2625 * Math.Sqrt(3003) *
            Math.Sqrt(423141891046875 * x * x - 75188243917358 * x + 3341471513803) +
            2959034203125 * x - 262895957753, 0.3333333333333333) / (525 * Math.Pow(143, 0.6666666666666667)) - 128266 * Math.Pow(97, 0.6666666666666667) / (525 * Math.Pow(143, 0.3333333333333333) *
            Math.Pow(2625 * Math.Sqrt(3003) * Math.Sqrt(423141891046875 * x * x -
            75188243917358 * x + 3341471513803) + 2959034203125 * x -
            262895957753, 0.3333333333333333)) - 194.0 / 525.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 57)
            {
                R1 = 1.0 / 495.0 * Math.Pow(2.0 / 127.0, 0.6666666666666667) * Math.Pow(23, 0.3333333333333333) *
            Math.Pow(1485 * Math.Sqrt(635) * Math.Sqrt(2732872967710875 * x * x -
            483939667247762 * x + 21437352465187) + 1956244071375 * x -
            173206752773, 0.3333333333333333) - 233228 * Math.Pow(2.0 / 127.0, 0.3333333333333333) *
            Math.Pow(23, 0.6666666666666667) / (495 * Math.Pow(1485 * Math.Sqrt(635) *
            Math.Sqrt(2732872967710875 * x * x - 483939667247762 * x + 21437352465187)
            + 1956244071375 * x - 173206752773, 0.3333333333333333)) - 184.0 / 495.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 58)
            {
                R1 = Math.Pow(41.0 / 13.0, 0.3333333333333333) * Math.Pow(573 * Math.Sqrt(17763) *
            Math.Sqrt(3788747330228172 * x * x - 567560672536624 * x + 21281442030543)
            + 4700679069762 * x - 352084784452, 0.3333333333333333) / (573 * Math.Pow(31, 0.6666666666666667)) - 89107 * Math.Pow(13.0 / 31.0, 0.3333333333333333) * Math.Pow(41, 0.6666666666666667) /
            (573 * Math.Pow(573 * Math.Sqrt(17763) * Math.Sqrt(3788747330228172 * x * x -
            567560672536624 * x + 21281442030543) + 4700679069762 * x -
            352084784452, 0.3333333333333333)) - 164.0 / 573.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 59)
            {
                R1 = Math.Pow(52 * Math.Sqrt(19) * Math.Sqrt(63471567276 * x * x - 9529554177 * x
            + 358298480) + 57104424 * x - 4286799, 0.3333333333333333) / (13 * Math.Pow(19, 0.6666666666666667)) - 1181.0 / (13 * Math.Pow(19, 0.3333333333333333) * Math.Pow(52 * Math.Sqrt(19) *
            Math.Sqrt(63471567276 * x * x - 9529554177 * x + 358298480) + 57104424 * x
            - 4286799, 0.3333333333333333)) - 3.0 / 13.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 60)
            {
                R1 = Math.Pow(122.0 / 13.0, 0.3333333333333333) * Math.Pow(309 * Math.Sqrt(34917) *
            Math.Sqrt(7194447923672997 * x * x - 1063822253873158 * x +
            39412745362197)
            + 4897513903113 * x - 362090624191, 0.3333333333333333) / (309 * Math.Pow(113, 0.6666666666666667)) - 49868 * Math.Pow(13.0 / 113.0, 0.3333333333333333) * Math.Pow(122, 0.6666666666666667)
            / (309 * Math.Pow(309 * Math.Sqrt(34917) * Math.Sqrt(7194447923672997 * x * x -
            1063822253873158 * x + 39412745362197) + 4897513903113 * x -
            362090624191, 0.3333333333333333)) - 122.0 / 309.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 61)
            {
                R1 = Math.Pow(253.0 / 13.0, 0.3333333333333333) * Math.Pow(11664 * Math.Sqrt(254) *
            Math.Sqrt(23548480764621696 * x * x - 3500212915673071 * x +
            130263729980737) + 28526324366592 * x - 2120056278421, 0.3333333333333333) /
            (648 * Math.Pow(127, 0.6666666666666667)) - 107837 * Math.Pow(13.0 / 127.0, 0.3333333333333333) *
            Math.Pow(253, 0.6666666666666667) / (648 * Math.Pow(11664 * Math.Sqrt(254) *
            Math.Sqrt(23548480764621696 * x * x - 3500212915673071 * x +
            130263729980737) + 28526324366592 * x - 2120056278421, 0.3333333333333333)) -
            253.0 / 648.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 62)
            {
                R1 = Math.Pow(233, 0.3333333333333333) * Math.Pow(663 * Math.Sqrt(3417) *
            Math.Sqrt(4557937198383972 * x * x - 680709759905188 * x + 25447185695527)
            + 2616496669566 * x - 195381676207, 0.3333333333333333) / (663 * Math.Pow(134, 0.6666666666666667)) - 115301 * Math.Pow(233, 0.6666666666666667) / (663 * Math.Pow(134, 0.3333333333333333)
            *
            Math.Pow(663 * Math.Sqrt(3417) * Math.Sqrt(4557937198383972 * x * x -
            680709759905188 * x + 25447185695527) + 2616496669566 * x -
            195381676207, 0.3333333333333333)) - 233.0 / 663.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 63)
            {
                R1 = Math.Pow(247.0 / 17.0, 0.3333333333333333) * Math.Pow(233 * Math.Sqrt(699) *
            Math.Sqrt(73681227929005056 * x * x - 8359414350696576 * x +
            237532963902107) + 1672141156704 * x - 94855373442, 0.3333333333333333) /
            (2796 * Math.Pow(3, 0.6666666666666667)) - 42433 * Math.Pow(17.0 / 3.0, 0.3333333333333333) *
            Math.Pow(247,
            0.6666666666666667) / (2796 * Math.Pow(233 * Math.Sqrt(699) * Math.Sqrt(73681227929005056
            * x
            * x - 8359414350696576 * x + 237532963902107) + 1672141156704 * x -
            94855373442, 0.3333333333333333)) - 247.0 / 699.0;
                Res = (uint)Math.Round(R1);
            }
            else if (a == 64)
            {
                R1 = Math.Pow(89.0 / 17.0, 0.3333333333333333) * Math.Pow(6561 * Math.Sqrt(82) *
            Math.Sqrt(6859294910190792 * x * x - 781387349855332 * x + 22279485169319)
            + 4920584584068 * x - 280268059489, 0.3333333333333333) / (486 * Math.Pow(41,
            0.6666666666666667)) - 44453 * Math.Pow(17.0 / 41.0, 0.3333333333333333) * Math.Pow(89, 0.6666666666666667) / (486
            * Math.Pow(6561 * Math.Sqrt(82) * Math.Sqrt(6859294910190792 * x * x -
            781387349855332 * x + 22279485169319) + 4920584584068 * x -
            280268059489, 0.3333333333333333)) - 89.0 / 243.0;
                Res = (uint)Math.Round(R1);
            }
        }
        catch { }
        return Res;
    }

    private bool FuncFirst(object obj, uint RandProg)
    {
        double p;
        bool Bool1 = false;
        uint y = 0;
        byte[] btedata = new byte[70];
        btedata = (byte[])obj;
        try
        {
            p = BitConverter.ToDouble(btedata, 0);
            y = Func64(p, 12);
            if (y == RandProg)
                Bool1 = true;
            else
                Bool1 = false;
        }
        catch { }
        return Bool1;
    }
}
