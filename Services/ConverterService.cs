using Grpc.Core;

namespace ConvertCurrencyService.Services
{
    public class ConverterService : Converter.ConverterBase
    {
        private readonly ILogger<ConverterService> _logger;

        public ConverterService(ILogger<ConverterService> logger)
        {
            _logger = logger;
        }


        private CoverterReply ConvertCurrencyNumberToWords(ConverterRequest request)
        {
            CoverterReply coverterReply = new CoverterReply();

            try
            {
                string trimmedNumber = String.Concat(request.CurrencyInNumber.Where(c => !Char.IsWhiteSpace(c))); // remove all the white spaces
                int centPoint = trimmedNumber.IndexOf(","); // find the index of cents start point (index of ',')
                bool isDollarSingle = false;
                bool isCentSingle = false;
                bool isCentNeeded = true;

                // if cents are available
                if (centPoint > 0)
                {
                    string dollarNumber = trimmedNumber.Substring(0, centPoint); // dollars in number
                    string centsNumber = trimmedNumber.Substring(centPoint + 1); // cents in number

                    try
                    {
                        int dollar = Convert.ToInt16(dollarNumber);
                        if (dollar == 1)
                        {
                            isDollarSingle = true;
                        }
                    }
                    catch
                    {
                        return new CoverterReply
                        {
                            CurrencyInWords = "",
                            Status = new ResponseStatus
                            {
                                Status = "fail",
                                Message = "Input Error, Input must be a number"
                            }
                        };
                    }


                    try
                    {
                        int cent = Convert.ToInt16(centsNumber);
                        if (centsNumber.Length == 1)
                        {
                            centsNumber += "0";
                        }
                        else if (centsNumber.Length > 2)
                        {
                            return new CoverterReply
                            {
                                CurrencyInWords = "",
                                Status = new ResponseStatus
                                {
                                    Status = "fail",
                                    Message = "Input Error, Pleace enter a value to cent amount between 0 to 99"
                                }
                            };
                        }
                        else if (cent == 1)
                        {
                            isCentSingle = true;
                        }
                        else if(cent == 0)
                        {
                            isCentNeeded = false;
                        }
                    }
                    catch
                    {
                        return new CoverterReply
                        {
                            CurrencyInWords = "",
                            Status = new ResponseStatus
                            {
                                Status = "fail",
                                Message = "Input Error, Pleace enter a value to cent amount between 0 to 99, It must be a number"
                            }
                        };
                    }

                    coverterReply.CurrencyInWords = ConvertNumberToWords(dollarNumber) + (isDollarSingle?"dollar ": "dollars ") +(isCentNeeded?"and "+ ConvertNumberToWords(centsNumber) + (isCentSingle ? "cent" : "cents"):"") ;

                }
                // if only dollars ( without cents )
                else
                {
                    try
                    {
                        int dollar = Convert.ToInt16(trimmedNumber);
                        if (dollar == 1)
                        {
                            isDollarSingle = true;
                        }
                    }
                    catch
                    {
                        return new CoverterReply
                        {
                            CurrencyInWords = "",
                            Status = new ResponseStatus
                            {
                                Status = "fail",
                                Message = "Input Error, Input must be a number"
                            }
                        };
                    }
                    coverterReply.CurrencyInWords = ConvertNumberToWords(trimmedNumber) + (isCentSingle?"dollar":"dollars");
                }

                coverterReply.Status = new ResponseStatus
                {
                    Status = "success",
                    Message = ""
                };
                return coverterReply;
            }
            catch (Exception ex)
            {
                return new CoverterReply
                {
                    CurrencyInWords = "",
                    Status = new ResponseStatus
                    {
                        Status = "fail",
                        Message = ex.Message
                    }
                };
            }

        }

        private string ConvertNumberToWords(string number)
        {
            string numberInWords = "";

            try
            {
                bool AllNumbersAreConverted = false;
                double numberInDouble = Convert.ToInt32(number);
                if (numberInDouble >= 0 && numberInDouble <= 999999999)
                {
                    number = numberInDouble.ToString();
                    int numberLength = number.Length;
                    string positionWord = "";
                    int breakingLength = 0;
                    if (numberInDouble >= 0)
                    {
                        switch (numberLength)
                        {
                            case 1:
                                numberInWords = oneToNine(number);
                                AllNumbersAreConverted = true;
                                break;
                            case 2:
                                numberInWords = tenTo99(number);
                                AllNumbersAreConverted = true;
                                break;
                            case 3:
                                positionWord = "hundred ";
                                breakingLength = 1;
                                break;
                            case 4:
                                positionWord = "thousand ";
                                breakingLength = 1;
                                break;
                            case 5:
                                positionWord = "thousand ";
                                breakingLength = 2;
                                break;
                            case 6:
                                positionWord = "thousand ";
                                breakingLength = 3;
                                break;
                            case 7:
                                positionWord = "million ";
                                breakingLength = 1;
                                break;
                            case 8:
                                positionWord = "million ";
                                breakingLength = 2;
                                break;
                            case 9:
                                positionWord = "million ";
                                breakingLength = 3;
                                break;
                            default:
                                AllNumbersAreConverted = true;
                                break;
                        }
                        if (!AllNumbersAreConverted)
                        {
                            if (breakingLength > 0)
                            {
                                numberInWords = ConvertNumberToWords(number.Substring(0, breakingLength)) + positionWord + ConvertNumberToWords(number.Substring(breakingLength));
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Input Error, Pleace enter a value to dollar amount between 0,00 to 999999999,99 , It must be a number");
                }
            }
            catch 
            {
                throw new Exception("Invalid input, Please input a valid number");
            }

            return numberInWords;
        }

        private string tenTo99(string number)
        {
            string numberInWords = "";
            int convertedNumber = Convert.ToInt32(number);
            switch (convertedNumber)
            {
                case 10:
                    numberInWords = "ten ";
                    break;
                case 11:
                    numberInWords = "eleven ";
                    break;
                case 12:
                    numberInWords = "twelve ";
                    break;
                case 13:
                    numberInWords = "thirteen ";
                    break;
                case 14:
                    numberInWords = "fourteen ";
                    break;
                case 15:
                    numberInWords = "fifteen ";
                    break;
                case 16:
                    numberInWords = "sixteen ";
                    break;
                case 17:
                    numberInWords = "seventeen ";
                    break;
                case 18:
                    numberInWords = "eighteen ";
                    break;
                case 19:
                    numberInWords = "nineteen ";
                    break;
                case 20:
                    numberInWords = "twenty-";
                    break;
                case 30:
                    numberInWords = "thirty-";
                    break;
                case 40:
                    numberInWords = "fourty-";
                    break;
                case 50:
                    numberInWords = "fifty-";
                    break;
                case 60:
                    numberInWords = "sixty-";
                    break;
                case 70:
                    numberInWords = "seventy-";
                    break;
                case 80:
                    numberInWords = "eighty-";
                    break;
                case 90:
                    numberInWords = "ninty-";
                    break;
                default:
                    if (convertedNumber > 10)
                    {
                        numberInWords = tenTo99(number.Substring(0, 1) + "0") + oneToNine(number.Substring(1));
                    }
                    break;

            }
            return numberInWords;
        }

        private string oneToNine(string number)
        {
            string numberInWords = "";
            int convertedNumber = Convert.ToInt32(number);
            switch (convertedNumber)
            {
                case 0:
                    numberInWords = "zero ";
                    break;
                case 1:
                    numberInWords = "one ";
                    break;
                case 2:
                    numberInWords = "two ";
                    break;
                case 3:
                    numberInWords = "three ";
                    break;
                case 4:
                    numberInWords = "four ";
                    break;
                case 5:
                    numberInWords = "five ";
                    break;
                case 6:
                    numberInWords = "six ";
                    break;
                case 7:
                    numberInWords = "seven ";
                    break;
                case 8:
                    numberInWords = "eight ";
                    break;
                case 9:
                    numberInWords = "nine ";
                    break;
            }
            return numberInWords;
        }

        public override Task<CoverterReply> ConvertCurrency(ConverterRequest request, ServerCallContext context)
        {
            CoverterReply coverterReply;
            try
            {
                coverterReply = ConvertCurrencyNumberToWords(request);
            }
            catch (Exception ex)
            {
                coverterReply = new CoverterReply
                {
                    CurrencyInWords = "",
                    Status = new ResponseStatus
                    {
                        Status = "fail",
                        Message = ex.Message,
                    }
                };
            }

            return Task.FromResult(coverterReply);
        }
    }
}
