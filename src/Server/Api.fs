module Api

    open Domain
    open Shared.Domain
    open Shared.Api
    

    let pokerApi (gameEngine:GameEngine) : IPokerApi = {
        getState    = fun id -> 
            async { return! gameEngine.GetState id  }
        createGame  = fun admin -> 
            async { return! gameEngine.CreateGame admin  }
        endGame     = fun id player -> 
            async { return! gameEngine.EndGame id player  }
        joinGame    = fun id player -> 
            async { return! gameEngine.JoinGame id player  }
        leaveGame   = fun id currentPlayer playerToLeave -> 
            async { return! gameEngine.LeaveGame id currentPlayer playerToLeave  }
        startRound  = fun id player -> 
            async { return! gameEngine.StartRound id player }
        finishRound = fun id player -> 
            async { return! gameEngine.FinishRound id player }
        playCard    = fun id player cardValue -> 
            async { return! gameEngine.PlayCard id player cardValue }
        
    }
