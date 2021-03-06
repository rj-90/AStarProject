﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics; //to messure perforamance

public class PathFinding : MonoBehaviour {


///***********************************************    Disclaimer **********************************///
//***please note, as a basis for our project we have used the tutorials mentioned and linked in class *//
//*** we have improved upon them to meet our needs. and to futher clerfy the task at hand. ***//
//***https://www.youtube.com/watch?v=T0Qv4-KkAUo&list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW&index=6***//

///***********************************************  class members  **********************************///

	Grid mygrid ; 
	public Transform target, Findingbot, t1, t2;
	public Transform newTarger;
	public List<Node> openList;
	public HashSet<Node> closedList;

	Stopwatch s = new Stopwatch();
///***********************************************  class methods   **********************************///

	void  Awake(){
	 mygrid = FindObjectOfType<Grid>();//get a refrence to the grid componenet 
	}

	// at the  start of the game and after the grid has been set up set and cacluate estimated costs based on gird locations and objects( teleporters, goal, bot) 
	void Start(){

		SetGoals();

	}
	//when the player hits space it will start path fidning 
	public void Update(){
		
		if(Input.GetKeyDown(KeyCode.Space)){
		//testomg time via stopwatch for astar alone - not accurate to an extream point as its for pathfinding and not initial setup 
		s.Start();
		AstarPathFinding(Findingbot.position, newTarger.position);
		s.Stop();
		UnityEngine.Debug.Log("elaps time for a star "+ s.ElapsedMilliseconds.ToString());		
		}

		//when theplayer hits x it will recalulate and reset goals 
		if(Input.GetKeyDown(KeyCode.X)){

			SetGoals();
		}

	
	
	}
	//  method to set up the iniital targetby calling and calucating hcosts - sends in represntions nodes of transporters goal and start node from inspector 
	public void SetGoals(){
		Node startNode = mygrid.GetNodeLocation(Findingbot.position);
		Node goalNode = mygrid.GetNodeLocation(target.position);
		Node firstTransporter = mygrid.GetNodeLocation(t1.position);											//Debug.Log("start node "+ start_pos.ToString());
		Node secondTransporter = mygrid.GetNodeLocation(t2.position);	

		//cacuate the h cost and determaine target before path planning
		getPortHcost(startNode, goalNode, firstTransporter, secondTransporter);	
																												//Debug.Log(newTarger.position.ToString() + "new target postion ");

	}
	//
	/// <summary>
	/// A star  path finding.
	/// finds and returns the path from a start pos to a targe postion 
	/// </summary>
	/// <param name="start_pos">Start position.</param>
	/// <param name="target_pos">Target position.</param>
	public void AstarPathFinding( Vector3 start_pos, Vector3 target_pos){

		//set up start and end node repristions 
		Node startNode = mygrid.GetNodeLocation(start_pos);
		Node goalNode = mygrid.GetNodeLocation(target_pos);

		//create a list of open nodes 
		openList = new List<Node>();
		closedList = new HashSet<Node>();																			//used hashset as it eliminates duplicates? check this further for better ds 

		//Put S into Open  
		openList.Add(startNode);
		while( openList.Count> 0){	//run as long as the list is not empy
																													//Debug.Log("entred openlist loop");
		    //assign the current node to the first element in the open list
			Node temp = openList[0];
			
			for(int i = 1 ; i < openList.Count ;i++){																//go through the open list
																													//Debug.Log("entred main for loop for open list ");
																													//Debug.Log("main loop counter nd the ioen list has "+ openList.Count.ToString());
				if(openList[i].fCost < temp.fCost || openList[i].fCost == temp.fCost 								//cehck elemnt in open list's  f cost < than the cirrent node's fcost 
				&&  openList[i].hCost < temp.hCost){																// or it is equal to it AND it has less h cost 
		
					temp = openList[i];
																													//if condition holds - assign current/temp node to new current node
																													//if it not the 'start' node itself assign our current node/ tmep to it 
						}
					}
			// remove current node from the open list and add it to the closed list 
			openList.Remove(temp);
			closedList.Add(temp);

			if(temp == goalNode){																										//	Debug.Log("Found goal node  ");
				getPathBack(startNode, goalNode); //return path from start to goal 
				return;

			}

			//for each loop through the neigboots of our temp/current if ( path is not found) 
			//chec kif the neighboot is not walkable or naibor is in closed list 
			//then move to other neighbor 


			//foreach neighbor of the current node 
			foreach (Node n in getNeighbuors(temp)){
																						//Debug.Log("entred nehghnoot loop in main loop in astar  ");
				if(! n.walkable || closedList.Contains(n)){
																						//Debug.Log("neigbot is not walkable or in closed list ");
					continue;
				}
				//cost from cuttent - neighbor 
				int newMoveCost = temp.gCost + getDistance(temp, n);
				//if our new move cost has less than our iterated node n's gcost or it is not in the open lis  - set it gcost to the the gnode - calcualye the ditance to the end node 
				if( newMoveCost < n.gCost || !openList.Contains(n)){

					n.gCost = newMoveCost;					//se gcost to have the new movment cost 
					n.hCost = getDistance(n, goalNode); //calcuate distance to end node 
					n.parentNode = temp;				// set out parent node(pointer) to temp/current node
																										//Debug.Log("assigned parent node to temp ");
					if(! openList.Contains(n)){				//at teh end of it if it doesnt have n in the open list 
																										//Debug.Log("adding neighboor to open list  ");
						openList.Add(n); }						// add the current checked node( from the neighnot lst) to the open list 
				} //end if cost check  							//loop back 

			}//end of foreach loop 
		}//end of main while lopp 
	}//end of A star 

	public void getPathBack(Node startingNode, Node EndingNode){

		//Debug.Log("entred get path back ");
		List<Node> gneratedPath = new List<Node>();
		Node temp = EndingNode;

		while(temp != startingNode){
			//Debug.Log("adding nodes as twmo != starting node ");

			gneratedPath.Add(temp);
			//Debug.Log("adding node   "+ temp.parentNode.worldPosition.ToString() + " added to the list ");
			temp = temp.parentNode;

			}

		//path is reversed - need to revierse it 
		gneratedPath.Reverse();

		//need gizmos to visulize it 
		mygrid.path = gneratedPath;
		//for debugging purposes 
		/*
		if(mygrid.path!=null){
		Debug.Log("PATH HAS HITEMS ");
			foreach( Node n in gneratedPath){
				Debug.Log("the path has node:  "+ n.worldPosition.ToString() + " added to it ");
		}
			Debug.Log("the path has "+ gneratedPath.Count.ToString() + "in the list");
		}*/
		//to  draw it 
		mygrid.DrawPath();
	}



	// to check neighbors of node - using list 
	/// <summary>
	/// Gets the neighbuor - returns a list of negibors of the current node 
	/// </summary>
	/// <returns>The neighbuors.</returns>
	/// <param name="node">current node.</param>
	public  List<Node> getNeighbuors(Node node){
	//	Debug.Log("enred get neghboors method  ");
		//create an empty list for the neghboors 
		List<Node> neighboors = new List<Node>();
		for (int x = -1 ; x<=1 ; x++){

			for (int y = -1 ; y <=1 ; y++){
				for (int z = -1 ; z <=1 ; z++){

					//Debug.Log("in neghboots loop  ");

					if( x== 0 && y == 0 && z == 0)
						continue; 


			//		Debug.Log("managed to continue  ");
					int checkX = node.xGridLocation + x;
					int checky = node.yGridLocation + y;
					int checkz = node.ZGridLocation + z;

				//	Debug.Log("got temp xyz postions  ");
					//if it is in the gird 
					if(checkX >= 0 && checkX < mygrid.gridSizeX 
						 && checky >= 0 && checky < mygrid.gridSizeY 
						&& checkz >= 0 && checkz < mygrid.gridSizeZ ){
							//add node to neighboor list 
							neighboors.Add(mygrid.grid[checkX,checky,checkz]);
					}//end 
			}//end of z inner loop 
		}
	}		//reutn list of neghboors 

			return neighboors;
	}//end of neghboor method 

	/// <summary>
	/// to calcualte costs with gravity constraints 
	/// </summary>
	/// <returns>The distance/cost ebtween two ndoes a and b, current node to next node  .</returns>
	/// or returns distanc/cost ebwteen current node and goal node - dependent on what we send from path fidning  --- 
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	public int getDistance( Node a, Node b){

		int xDistance = (Mathf.Abs(a.xGridLocation - b.xGridLocation))*2; //(weight of horizontal movement is standard)
		int temp = Mathf.Abs(a.yGridLocation - b.yGridLocation);
		int yDistance ;


		//gravity modifications 
		if (b.yGridLocation > a.yGridLocation){
			 yDistance = temp * 4; //Weight of moving up (most expensive)
		} 
		else {
			 yDistance = temp ; //Weight of moving down (cheapest)
		}

		int zDistance = (Mathf.Abs(a.ZGridLocation - b.ZGridLocation))*2;

		if(xDistance > zDistance) {
																						//	Debug.Log("returned distance calucaltiond from node a to b x > z ");
			return 14 * zDistance + 10 * ( xDistance - zDistance) + 10 * yDistance;
																						//Debug.Log("returned distance calucaltiond from node a to b  ");
		}
																						//Debug.Log("returned distance calucaltiond from node a to b  ");
		return 14 * xDistance + 10 * ( zDistance - xDistance) + 10 * yDistance;
																						//Debug.Log("returned distance calucaltiond from node a to b  ");
	}//end of method 




	/// <summary>
	/// calcualte initial costs to set target at the begining
	/// </summary>
	/// <returns>The port hcost.</returns>
	/// <param name="current">start node .</param>
	/// <param name="goal">Goal / end goal node .</param>
	/// <param name="_tran1">Transporter one as a node .</param>
	/// <param name="_tran2">Tran2. as a node </param>
	//
	public   void getPortHcost(Node current, Node goal , Node _tran1, Node _tran2){

	//current = start node 
		int  tranOneDistance, transTwoDistance;

		Node closenode;
		Node farnode ;
		Transform temp;

		int y1 = Mathf.Abs(current.yGridLocation - _tran1.yGridLocation);
		int x1 = Mathf.Abs(current.xGridLocation - _tran1.xGridLocation);
		int z1= Mathf.Abs(current.ZGridLocation - _tran1.ZGridLocation);

		int y2 = Mathf.Abs(current.yGridLocation - _tran2.yGridLocation);
		int x2 = Mathf.Abs(current.xGridLocation - _tran2.xGridLocation);
		int z2= Mathf.Abs(current.ZGridLocation - _tran2.ZGridLocation);

		tranOneDistance= x1+y1+z1;
		transTwoDistance= x2+y2+z2;

		if (tranOneDistance < transTwoDistance) {
			
			closenode=_tran1;
			farnode=_tran2;
			temp = t1;

			}
		else {
			closenode=_tran2;
			farnode=_tran1;
			temp = t2;
			}

	//	Debug.Log("teh closest trans porter is located at " + closenode.worldPosition);

		int goald, tran1d, tran2d;
		int yc = Mathf.Abs(current.yGridLocation - goal.yGridLocation);
		int xc = Mathf.Abs(current.xGridLocation - goal.xGridLocation);
		int zc= Mathf.Abs(current.ZGridLocation - goal.ZGridLocation);

		goald= yc+xc+zc;

		int yt1 = Mathf.Abs(current.yGridLocation - closenode.yGridLocation);
		int xt1 = Mathf.Abs(current.xGridLocation - closenode.xGridLocation);
		int zt1= Mathf.Abs(current.ZGridLocation - closenode.ZGridLocation);

		tran1d= yt1+xt1+zt1;
	//	Debug.Log("transporter one distance is" + tran1d.ToString());

		int yt2 = Mathf.Abs(goal.yGridLocation - farnode.yGridLocation);
		int xt2 = Mathf.Abs(goal.xGridLocation - farnode.xGridLocation);
		int zt2= Mathf.Abs(goal.ZGridLocation - farnode.ZGridLocation);

		tran2d= yt2+xt2+zt2;
//		Debug.Log("transporter two distance is" + tran2d.ToString());

	//	Debug.Log("goal distance is " +  goald.ToString() + " transporter disncatnces "+ (tran1d + tran2d).ToString() );
		if (goald <(tran1d + tran2d) ){

	//		Debug.Log("new target is the goal ");
			this.newTarger = target;
			}
		else{
	//		Debug.Log("new target is a transporter ");
			this.newTarger = temp;
		//	Debug.Log("setting transporter as goal");

			}
    }




	}//end of classs 
