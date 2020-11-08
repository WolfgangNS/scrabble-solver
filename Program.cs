using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace scrabblerobot
{
    class Program
    {
        static char[,] board = new char[15,15];
        static int[,] LM = //letter multipliers
        {{1,1,1,2,1,1,1,1,1,1,1,2,1,1,1},
	    {1,1,1,1,1,3,1,1,1,3,1,1,1,1,1},
		{1,1,1,1,1,1,2,1,2,1,1,1,1,1,1},
		{2,1,1,1,1,1,1,2,1,1,1,1,1,1,2},
		{1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
		{1,3,1,1,1,3,1,1,1,3,1,1,1,3,1},
		{1,1,2,1,1,1,2,1,2,1,1,1,2,1,1},
		{1,1,1,2,1,1,1,1,1,1,1,2,1,1,1},
		{1,1,2,1,1,1,2,1,2,1,1,1,2,1,1},
		{1,3,1,1,1,3,1,1,1,3,1,1,1,3,1},
		{1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
		{2,1,1,1,1,1,1,2,1,1,1,1,1,1,2},
		{1,1,1,1,1,1,2,1,2,1,1,1,1,1,1},
		{1,1,1,1,1,3,1,1,1,3,1,1,1,1,1},
		{1,1,1,2,1,1,1,1,1,1,1,2,1,1,1}};
        static List<string> words;
        static void Main(string[] args)
        {
            for(int i = 0;i<=14;i++){
                for(int j = 0;j<=14;j++){
                    board[i,j]= (char)32; //space
                }
            }
            var words = new List<string>();
            StreamReader sr = new StreamReader("/home/wolfgang/Documents/words.txt");
            for(int i = 0;i<=186029;i++){
                words.Add(sr.ReadLine());
            }

            string tiles = "AAAAAAAAABBCCDDDDEEEEEEEEEEEEFFGGGHHIIIIIIIIIJKLLLLMMNNNNNNOOOOOOOOPPQRRRRRRSSSSTTTTTTUUUUVVWWXYYZ";

            //DRAW TILES
            string shuffled = "";
            Random r = new Random(420692);
            //Random r = new Random(91169);
            string p1,p2 = "";

            while(tiles!=""){
                var i = r.Next(tiles.Length);
                shuffled += tiles[i];
                tiles = tiles.Remove(i,1);
            }
            tiles = shuffled; //naming getting confusing lol
            p1 = tiles.Substring(0,7);
            tiles = tiles.Remove(0,7);
            p2 = tiles.Substring(0,7);
            tiles = tiles.Remove(0,7);
            Console.WriteLine("{0}, {1}, {2}", p1, p2, tiles);
            //int p1_blanks, p2_blanks = 0; //for blank tiles



            var arrangements = new List<string>();            
            //arrangements.Add("ARSON");
            arrangements = longestwords(p2, ref words);
            arrangements = arrangements.OrderBy(x=>-x.Length).ToList<string>();
            
            //TO-DO: switch properly between p1 and p2 between turns
            
            //I need to evaluate the letter scores for each next
            int k = 0;
            foreach(char a in arrangements[0]){
                board[7,7+k] = a;
                k++;
                p2 = p2.Remove(p2.IndexOf(a),1);
            }
            print();

            //draw new tiles from the bag
            var diff = 7-p2.Length;
            p2 += tiles.Substring(0,diff);
            tiles = tiles.Remove(0,diff);
            Console.WriteLine("{0}, {1}, {2}", p1, p2, tiles);

            //next: evaluate positions to see if they're valid
            //next: evaluate letter scores for valid positions
            
            var rows = new List<string>();
            var cols = new List<string>();
            for(int i=0;i<=14;i++){
                string rowstring = "";
                string colstring = "";
                for(int j = 0;j<=14;j++){
                    rowstring+=board[j,i];
                    colstring+=board[i,j];
                }
                rows.Add(rowstring);
                cols.Add(colstring);
            }

            //now for the Regex magic
            var capture = new Regex("[A-Z]? ?([A-Z ]*[A-Z]+[A-Z ]*) ?[A-Z]?");
            var boardmatches = new List<string>();
            foreach(string focus in rows){
                try{
                string match = capture.Matches(focus)[0].Groups[0].Value; //will throw an error on this
                Console.WriteLine(match.Replace(" ","."));
                boardmatches.Add(match.Replace(" ",".?"));
                }catch{}
            }//TO-DO: repeat for columns
            arrangements.Clear();
            //I could speed this up by regex-ing each dictionary entry? 
            //But I need to speedtest that
            foreach(string pattern in boardmatches){
                var longwords = longestwords(p2+pattern.Replace(".?",""),ref words);
                foreach(string word in longwords){
                    if(Regex.IsMatch(word,pattern)){
                        arrangements.Add(word);
                    }
                }
                //the replace thing is a bit redundant
                //go back to the dictionary at this point to search by letter combinations?
            }
            arrangements = arrangements.OrderBy(x=>-x.Length).ToList<string>();
            Console.WriteLine(arrangements[0]);
            k = 0;
            var rownum = -1;
            var colnum = -1;
            foreach(string row in rows){
                if(boardmatches.Contains(row.Replace(" ",".?"))){
                    if(Regex.IsMatch(arrangements[0],row.Replace(" ",".?"))){
                    rownum = k;
                    colnum = row.IndexOf(Regex.Match(row,"[A-Z]").Value)-arrangements[0].IndexOf(Regex.Match(row,"[A-Z]").Value);
                    Console.WriteLine(rownum+", "+colnum+"\ncoordinates for debugging");                
                }
                }
                k++;
            }


            Console.WriteLine("Done.");

            //----------------------------------
            //--------END OF CODE---------------
            //----------------------------------

        }
        static void print(){
            for(int j = 0;j<15;j++){
                for(int i = 0;i<15;i++){
                    if(board[i,j]!=(char)32){
                        Console.Write(" " + board[i,j]);
                    }else{
                        Console.Write(" .");
                    }
            }
            Console.Write("\n");
            }

        }
        static List<string> longestwords(string tiles, ref List<string> words){
            //var "words" is screwy, the way it's initialized
            var arrangements = new List<string>();
            foreach(string word in words){
            var tiles_2 = tiles;
                try{
                foreach(char letter in word){
                    if(!tiles_2.Contains(letter)){
                        throw new Exception(); //there must be a better way to exit a loop
                    }
                    tiles_2 = tiles_2.Remove(tiles_2.IndexOf(letter),1);
                }
                arrangements.Add(word);
                }catch{}
            }
            //arrangements = arrangements.OrderBy(x=>-x.Length).ToList<string>(); //second letter
            //arrangements = arrangements.FindAll(x => x.Length==arrangements[0].Length);
            
            return arrangements;
        }
    }
    class Bot{
        List<char> tiles = new List<char>();
        //method firstmove
        //method regularmove

    }
    static class Board{ //do I need this?
        //properties: list of words, 
        //like an api thing, that enforces the rules and whatnot
    }
    //create class for each word, and it's properties?
    // -> like length, letters containing, 
    //linked list of words and their letters alphabetized?
    //or a tree of words?
    //or a tree of each word, letters alphabetized? (e.g. spider becomes ediprs)
}

//consulting between board, available tiles, and dictionary

//https://csharp.net-tutorials.com/linq/linq-query-syntax-vs-method-syntax/
//https://stackoverflow.com/questions/1175645/find-an-item-in-list-by-linq
