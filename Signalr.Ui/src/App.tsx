import React, { useEffect, useState } from 'react';
import './App.css';
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
function App() {
  const [text, setText] = useState<string>("");
  const [group, setGroup] = useState<string>("");
  const [msgList, setMsgList] = useState<Array<any>>([])
  const [hubConnection, setHubConnection] = useState<HubConnection>()
  useEffect(() => {
  }, [])
  const createHubConnection = async () => {
    const hubCn = new HubConnectionBuilder().withUrl("https://localhost:7289/chat").build()
    try {
      await hubCn.start();
      console.log(hubCn.connectionId)
      setHubConnection(hubCn)
      hubCn.invoke("Initialize", group).then((res) => { })

    } catch (e) {
      console.log("e", e)
    }
  }
  const login = async () => {
    await createHubConnection();
  }
  const sendMsg = () => {
    if (hubConnection) {
      hubConnection.invoke("SendMessage", text,group).then((res) => { })
    }
  }
  useEffect(() => {
    if (hubConnection) {
      hubConnection.on("ReceiveMessage", (mesaj: string) => {
        setMsgList((prevState) => {
          return prevState.concat(mesaj)
        }
        )
      }
      )
    }
  }, [hubConnection])
  return (
    <div className="App">
      <header className="App-header">
        <h6>Group</h6>
        <input value={group} onChange={(e) => { setGroup(e.target.value) }} />
        <button onClick={login}>Login </button>
        <h6>Message</h6>
        <input value={text} onChange={(e) => { setText(e.target.value) }} />
        <button onClick={sendMsg}>Send Message </button>
      </header>
      <div>
        <h2>Messages</h2>
        <ul>
          {msgList.map((item) => {
            return (
              <li>{item}</li>
            )
          })}
        </ul>
      </div>
    </div>
  );
}
export default App;