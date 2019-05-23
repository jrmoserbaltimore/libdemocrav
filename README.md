# libdemocrav
Core library for Electronic Management System (EMS) and Electronic Voting Machine (EVM) software.

# Why DemocraV?

The Democracy Voting software, published under MIT license, allows states and municipalities to implement modern elections with minimal cost.  The software carries minimal restrictions—thanks to the MIT license—and can greatly reduce costs and increase home rule for municipalities.

## Integrity Concerns

A tamper-resistant balloting process is essential to high-integrity elections.

When using electronic voting, we must prove the state of software on the voting machine.  This requires publication and validation of the software at the time of voting:  no trusted party can assure the voter they are not colluding to corrupt the election, and so we must prove to the voter that *anybody* with the time and skill can, independently and without permission, examine and find any form of tampering or defect *at any time in the future*.

Other methods of ballot integrity are possible, although they trade other risksi.  Nevertheless, all such methods must achieve this above form of universal verifiability to maintain election integrity.

This software allows elections authorities to impliment pure Direct Recording Electronic (DRE) processes and mixed hand-counted and electronic-tabulated (HCET) processes, such as the Fast Assisted-Hand-Count (FAHC) process, using a readily-available, no-cost software library they can freely distribute for public inspection.

## Representative and Tamper-Resistent Electoral Systems
Tamper-resistent balloting process alone cannot protect the integrity of an election.  Voting rules like Plurality, majority-runoff, and Instant Runoff Voting are prone to poor representation and easily manipulated by strategic nomination of candidates—not candidates intended to win, but candidates intended to make *other* candidates *lose*.  Approval and the fractional approval systems—score and rated systems—require enormous strategic thinking and betray the voter's intent at all times, as well.

This software supports legacy electoral systems, including Plurality and Instant Runoff Voting, as well as modern Single Transferable Vote and Tideman's Alternative.

The software implements alternatives to the dangerous, polarizing, and easily-manipulated Party Primary cycle and the non-representative top-two cycle.  It primarily enables the Unified Majority system—a nonpartisan blanket primary by STV nominating five to nine candidates, followed by a Tideman's Alternative general election for single-winner or an STV general election for multiple-winner—designed to reliably finds consensus and to resist social media propaganda attacks, strategic nomination attacks, and strategic bloc voting.

## Cost Factors
LibDemocraV is free software, and designed for low hardware demand when implemented as a DRE voting machine software or a console for FAHC.  FAHC implementations should aim for counting no fewer than 30 ballots per minute.

A polling center should serve at most 2,500 voters per day due to physical logistics—long lines are disenfranchising.  While we can protect Internet-based voting from hackers, a corrupt elections authority can *falsify an election not carried out in public view*, so high participation in in-person voting represents the core of a high-integrity election.  This means the cost of DRE voting machines and FAHC consoles drives the cost of an election, and minimizing those costs is critical.

The software is designed to avoid high costs and, eventually, to take the place of multi-million-dollar elections management systems.

For example, the State of Maryland issued a [fiscal note](https://legiscan.com/MD/supplement/HB624/id/96430) describing tabulation software for Instant Runoff Voting as a $385,000 cost, and integration with an Election Management System—simply allowing the storage and retrieval of ranked ballots along with all currently-supported ballot types—at $800,000.

Under State law, a municipality can implement its own municipal elections using ranked ballots.  It's technically-legal for the local Board of Elections to tabulate these ballots, publish the ballots in full and the tabulation software and algorithms, and send the results to the State for publication.  This software allows the municipality to operate its own local elections without incurring a $1.2 million expense at the State level for costs related to the core EMS and tabulators.

In the long run, this software will provide full EMS capabilities with voter registration management, transparency reporting, and integrity controls.  Operating procedures, rather than software, provide most of that integrity; the software is designed with such procedures in mind.  States may find significant savings by migrating their elections to a cost-free software product and changing their elections services vendors at will, rather than locking into the vendor providing the software.

# Voting Rules

For single-seat elections, this library provides the following vote count rules (recommended marked in bold):

* **Tideman's Alternative**
* Schulze
* Instant Run-off Voting
* Plurality

Schulze is suitable for parliamentary groups, although the voting rule is mathematically-complex.  Our default Tideman's Alternative configuration elects from the Schwartz Set, and so produces similar results to Schulze.

Instant Run-off Voting can elect the second-place Condorcet loser, and so is manipulable with moderate effort.  This failure occurred in the penultimate round of the 2009 Burlington Mayoral Election, in which the Condorcet and Plurality winners both lost—raising the question of what exactly makes a candidate the IRV winner.  It is not recommended.

Plurality is subject to trivial manipulation and cannot produce representative results.  Although popular and the simplest system, it is a complete failure for democracy.

For multiple-seat elections, this library provides the following vote count rules:

* **Single Transferable Vote** (STV)
  * **Meek STV**
  * **Schulze STV**
* Multiple Non-Transferable Vote (MNTV)

Meek-STV uses weighted run-off rounds.  Every time a candidate is eliminated, Meek-STV adjusts the election to function as if the candidate was never a candidate.  It also recalculates the quota of votes required to win each time a candidate is elected.  This resists certain manipulation attacks—notably Woodall Free Riding—and ensures that a voter whose first choice is an early winner isn't put at a disadvantage compared to a voter whose first choice is a late loser and *second* choice is the same winning candidate.

In other words, if two voters cast these ballots:

* Alice > Bob > Chris
* Chris > Alice > Bob

Under many STV rules, if Alice wins easily and Chris is then eliminated, the second ballot skips over Alice and carries one full vote to Bob, while the first ballot transfers a *partial* vote to Bob.  We can say that the second voter is *free riding* on Chris.

Under Meek-STV, when Chris is eliminated, the votes are recounted without consideration to Chris at all, and both of these voters transfer the same portion of a vote to Bob.

It's possible for the second voter to simply not rank Alice at all, under the assumption Alice will win, called Hylland Free Riding.  This is hazardous—with enough voters doing this, Alice may lose—but mathematically-possible.  Schulze-STV extends any other STV rule, such as Meek-STV, by comparing outcomes and identifying which outcome is most strongly representative.  It resists Hylland Free Riding in most, but not all, conditions, and provides a mechanism for replacing an elected official fairly when a single seat is vacated during a term.

Compared to Single Transferable Vote, Multiple Non-Transferable Vote tends to elect all winners based on a majority—especially when Party Primary has selected only as many from each party as there are winners.  The minority vote gets no representation because of this.  MNTV is vulnerable to simple vote splitting, and too many candidates can cause the most popular candidates to weaken each other and lose to less-popular candidates.  It is a common, simple, and obvious system, but hopelessly damaged and detrimental to democracy.
