# SmartTicketApi

## Back-end of my final degree project in .Net 7. 

It is an API that allows the creation of events of any type. Likewise, this API is connected to the Ethereum blockchain on which a Smart Contract is deployed every time a new event is created. The smart contract is used to manage the sale of tickets for that event, where each ticket corresponds to an NFT (ERC-721 token). 
By using the NFTs, the system ensures the ownership of a ticket, making any kind of resale useless. The way to verify the ownership is using the same smart contract where given the address of a user's wallet we can know if a user has an entry of that event and if the entry he has corresponds to that event.
