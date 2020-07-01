# fps_bots_view
**Visualization in _Unity3D_ for a game model found under _fps_bots repository_ (bettinabundik/fps_bots)**

! video coming soon !

### Model
- AI induced FPS bots - Master's Thesis and Project (March 2020, Waseda University, Tokyo, Japan)
- Game model is based on the project and paper of my master's thesis on: *Features and Performance of Sarsa Reinforcement Learning Algorithm with Eligibility Traces and Local Environment Analysis for Bots in First Person Shooter Games*
- Written in C# .NET Framework 4.5.2 (Visual Studio 2019)

### View
- Visualization for game model of AI induced FPS bots which shows the *replay* of the training process
- Purpose:
  - To follow and examine behaviours of agents individually so that different strategies (developed through time) can be recognized and evaluated
  - To see how the agents actually play after learning - some features of their behaviour cannot be seen well enough through statistical data
  - To find remaining errors in the code of the model or in the training process itself
- The replay is only for the learning process of the *ultimate environment* (training on other maps is conducted before this and the learned data is used for the agents' ultimate training)
- Created in Unity3D 2019.3.15f1 and C# (same version as above)
  - Personal version of Unity
  - Free-to-use assets were used from Unity's Asset Store, special thanks to the creators!
