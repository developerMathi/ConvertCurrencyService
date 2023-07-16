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
                bool isDollarSingle = false; // to find dollar or dollars
                bool isCentSingle = false; // to find cent or cents
                bool isCentNeeded = true; // to remove cent part if cent = 0

                // if cents are available
                if (centPoint > 0)
                {
                    string dollarNumber = trimmedNumber.Substring(0, centPoint); // dollars in number
                    string centsNumber = trimmedNumber.Substring(centPoint + 1); // cents in number

                    try
                    {
                        int dollar = Convert.ToInt32(dollarNumber);
                        // if only one dollar
                        if (dollar == 1)
                        {
                            isDollarSingle = true;
                        }
                    }
                    // if error in convert dollar string into number
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
                        // if cent length is one attach a 0 in the end
                        if (centsNumber.Length == 1)
                        {
                            centsNumber += "0";
                        }
                        // if cents > 99 assign error message
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
                        // if only one cent
                        else if (cent == 1)
                        {
                            isCentSingle = true;
                        }
                        // if zero cents
                        else if (cent == 0)
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
                    // convert number into words
                    coverterReply.CurrencyInWords = ConvertNumberToWords(dollarNumber) + (isDollarSingle ? "dollar " : "dollars ") + (isCentNeeded ? "and " + ConvertNumberToWords(centsNumber) + (isCentSingle ? "cent" : "cents") : "");

                }
                // if only dollars ( without cents )
                else
                {
                    try
                    {
                        int dollar = Convert.ToInt16(trimmedNumber);
                        // if only one dollar
                        if (dollar == 1)
                        {
                            isDollarSingle = true;
                        }
                    }
                    // error handling when convert string to int
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
                    // convert number to words
                    coverterReply.CurrencyInWords = ConvertNumberToWords(trimmedNumber) + (isCentSingle ? "dollar" : "dollars");
                }
                // reply message status
                coverterReply.Status = new ResponseStatus
                {
                    Status = "success",
                    Message = ""
                };
                return coverterReply;
            }
            // error handling
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

        // convert number into words
        private string ConvertNumberToWords(string number)
        {
            string numberInWords = "";

            try
            {
                bool AllNumbersAreConverted = false; // variable to find fully converted or not
                double numberInDouble = Convert.ToInt32(number); // number string to int
                if (numberInDouble >= 0 && numberInDouble <= 999999999) // number validation
                {
                    number = numberInDouble.ToString(); // number int to string now unwanted 0s will be removed
                    int numberLength = number.Length; // length of number string
                    string positionWord = ""; // hundred, thousand, million
                    int breakingLength = 0; // find the separating postitions
                    if (numberInDouble >= 0)
                    {
                        switch (numberLength)
                        {
                            case 1:
                                numberInWords = oneToNine(number); // convert number to word 0-9
                                AllNumbersAreConverted = true;
                                break;
                            case 2:
                                numberInWords = tenTo99(number); // convert number to word 10-99
                                AllNumbersAreConverted = true;
                                break;
                            case 3:
                                positionWord = "hundred "; // hundred
                                breakingLength = 1; // if 999- break as 9 99
                                break;
                            case 4:
                                positionWord = "thousand "; // thousand
                                breakingLength = 1; // if 9999 break as 9 thousand 999
                                break;
                            case 5:
                                positionWord = "thousand ";
                                breakingLength = 2; // if 99999 break as 99 thousand 999
                                break;
                            case 6:
                                positionWord = "thousand ";
                                breakingLength = 3; // if 999999 break as 999 thousand 999
                                break;
                            case 7:
                                positionWord = "million ";
                                breakingLength = 1; // if 9999999 break as 9 million 999999
                                break;
                            case 8:
                                positionWord = "million ";
                                breakingLength = 2; // if 99999999 break as 99 million 999999
                                break;
                            case 9:
                                positionWord = "million ";
                                breakingLength = 3; // if 999999999 break as 999 million 999999
                                break;
                            default:
                                AllNumbersAreConverted = true;
                                break;
                        }
                        // if not fully converted
                        if (!AllNumbersAreConverted)
                        {
                            if (breakingLength > 0)
                            {
                                // break the numbers with breaking points and call the recursive method to convert again
                                numberInWords = ConvertNumberToWords(number.Substring(0, breakingLength)) + positionWord + ConvertNumberToWords(number.Substring(breakingLength));
                            }
                        }
                    }
                }
                //error handling
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

        // method to convert the numbers 10-99 to words
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
                    numberInWords = "twenty ";
                    break;
                case 30:
                    numberInWords = "thirty ";
                    break;
                case 40:
                    numberInWords = "fourty ";
                    break;
                case 50:
                    numberInWords = "fifty ";
                    break;
                case 60:
                    numberInWords = "sixty ";
                    break;
                case 70:
                    numberInWords = "seventy ";
                    break;
                case 80:
                    numberInWords = "eighty ";
                    break;
                case 90:
                    numberInWords = "ninty ";
                    break;
                default:
                    if (convertedNumber > 10)
                    {
                        // numbers like 23,34 etc
                        numberInWords = tenTo99(number.Substring(0, 1) + "0").TrimEnd() + "-" + oneToNine(number.Substring(1));
                    }
                    break;

            }
            return numberInWords;
        }

        // method to convert the numbers 0-9 to words
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
                // convert currency to words
                coverterReply = ConvertCurrencyNumberToWords(request);
            }
            catch (Exception ex)
            {
                // if any exception return exception message
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
            //return results
            return Task.FromResult(coverterReply);
        }
    }
}
