# fringe-together

Sample query 

```graphql
{
    show(uri: "https://tickets.edfringe.com/whats-on/1-hour-of-insane-magic") {
      title
      location
      time
      duration
      date
      description

      availability {
        availableDates
      }
    }
}
```
